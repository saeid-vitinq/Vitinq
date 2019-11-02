namespace Umbraco.Web.PublishedContentModels
{
    using Our.Umbraco.Vorto.Extensions;

    using Umbraco.Core;
    using Umbraco.ModelsBuilder;

    /// <summary>
    /// The Main Page
    /// </summary>
    public partial class Master
    {
        /// <summary>
        /// Gets the url segment for a specific culture.
        /// </summary>
        /// <param name="culture">
        /// The culture
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetUrlSegment(string culture)
        {
            var urlSegment = this.GetVortoValue<string>("urlSegment", culture);

            if (string.IsNullOrEmpty(urlSegment))
            {
                urlSegment = this.UrlName;
            }

            return urlSegment.ToUrlSegment().EnsureEndsWith("/");
        }
    }
}