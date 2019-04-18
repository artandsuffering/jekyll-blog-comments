using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace JekyllBlogCommentsAzure
{
    /// <summary>
    /// Represents a Comment to be written to the repository in YML format.
    /// </summary>
    class Comment
    {
        struct MissingRequiredValue { } // Placeholder for missing required form values
        static readonly Regex validPathChars = new Regex(@"[^a-zA-Z0-9-]"); // Valid characters when mapping from the blog post slug to a file path

        /// <summary>
        /// Try to create a Comment from the form.  Each Comment constructor argument will be name-matched
        /// against values in the form. Each non-optional arguments (those that don't have a default value)
        /// not supplied will cause an error in the list of errors and prevent the Comment from being created.
        /// </summary>
        /// <param name="form">Incoming form submission as a <see cref="NameValueCollection"/>.</param>
        /// <param name="comment">Created <see cref="Comment"/> if no errors occurred.</param>
        /// <param name="errors">A list containing any potential validation errors.</param>
        /// <returns>True if the Comment was able to be created, false if validation errors occurred.</returns>
        public static bool TryCreateFromForm(NameValueCollection form, out Comment comment, out List<string> errors)
        {
            var constructor = typeof(Comment).GetConstructors()[0];
            var values = constructor.GetParameters()
                .ToDictionary(
                    p => p.Name,
                    p => ConvertParameter(form[p.Name], p.ParameterType) ?? (p.HasDefaultValue ? p.DefaultValue : new MissingRequiredValue())
                );

            errors = values.Where(p => p.Value is MissingRequiredValue).Select(p => $"Form value missing for {p.Key}").ToList();

            comment = errors.Any() ? null : (Comment)constructor.Invoke(values.Values.ToArray());
            var isFormValid = !errors.Any();
            var config = PostCommentToPullRequestFunction.config;

            return isFormValid;
        }

        private static object ConvertParameter(string parameter, Type targetType)
        {
            return String.IsNullOrWhiteSpace(parameter)
                ? null
                : TypeDescriptor.GetConverter(targetType).ConvertFrom(parameter);
        }

        public Comment(string post_id, string message, string name)
        {
            this.post_id = validPathChars.Replace(post_id, "-");
            this.message = message;
            this.name = name;

            date = DateTime.UtcNow;
            id = new { this.post_id, this.name, this.message, date }.GetHashCode().ToString("x8");
        }

        [YamlIgnore]
        public string post_id { get; }

        public string id { get; }
        public DateTime date { get; }
        public string name { get; }

        public string message { get; }

        public string ToYaml()
        {
            return new SerializerBuilder().Build().Serialize(this);
        }
    }
}
