﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultilingualUrlProvider.cs" company="Colours B.V.">
//   © Colours B.V. 2015
// </copyright>
// <summary>
//   The multilingual url provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ozaltin.UrlProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Umbraco.Core;
    using Umbraco.Core.Cache;
    using Umbraco.Core.Dictionary;
    //using Umbraco.Extensions.Models;
    using Umbraco.Web;
    using Umbraco.Web.PublishedContentModels;
    using Umbraco.Web.Routing;

    /// <summary>
    /// The multilingual url provider.
    /// </summary>
    public class MultilingualUrlProvider : IUrlProvider
    {
        /// <summary>
        /// The get url.
        /// </summary>
        /// <param name="umbracoContext">
        /// The umbraco context.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="current">
        /// The current.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetUrl(UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode)
        {
            UmbracoContext.Current.Application.ApplicationCache.RequestCache.ClearCacheItem("MultilingualUrlProvider-GetUrl-" + id);

            return UmbracoContext.Current.Application.ApplicationCache.RequestCache.GetCacheItem<string>(
                "MultilingualUrlProvider-GetUrl-" + id,
                () =>
                    {
                        var content = umbracoContext.ContentCache.GetById(id) as Umbraco.Web.PublishedContentModels.Master;

                        if (content != null)
                        {
                            var domains =
                                ApplicationContext.Current.Services.DomainService.GetAll(true)
                                    .OrderBy(x => x.Id)
                                    .ToList();

                            if (domains.Any())
                            {
                                // Don't use umbracoContext.PublishedContentRequest.Culture because this code is also called in the backend.
                                //var currentCulture = Thread.CurrentThread.CurrentCulture.ToString();
                                var currentCulture = new UmbracoHelper(umbracoContext).CultureDictionary.Culture.Name;
                                // On the frontend get the domain that matches the current culture. We could also check the domain name, but for now the culture is enough.
                                // Otherwise just get the first domain. The urls for the other domains are generated in the GetOtherUrls method.
                                //var domain = UmbracoContext.Current.IsFrontEndUmbracoRequest
                                //                 ? domains.First(x => x.LanguageIsoCode.InvariantEquals(currentCulture))
                                //                 : domains.First();

                                var domain = domains.First(x => x.LanguageIsoCode.InvariantEquals(currentCulture));

                                if (content.DocumentTypeAlias.InvariantEquals(MainPage.ModelTypeAlias))
                                {
                                    // Return the domain if we're on the homepage because on that node we've added the domains.
                                    return domain.DomainName.EnsureEndsWith("/");
                                }
                                // Get the parent url and add the url segment of this culture.
                                if (content.Parent != null)
                                {
                                    var parentUrl = umbracoContext.UrlProvider.GetUrl(content.Parent.Id);
                                    var urlSegment = content.GetUrlSegment(domain.LanguageIsoCode);
                                    return parentUrl.EnsureEndsWith("/") + urlSegment;
                                }
                                else
                                {
                                    return domain.DomainName.EnsureEndsWith("/") + content.GetUrlSegment(domain.LanguageIsoCode);
                                }


                            }
                        }

                        return null;
                    });
        }

        /// <summary>
        /// The get other urls.
        /// </summary>
        /// <param name="umbracoContext">
        /// The umbraco context.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="current">
        /// The current.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            return UmbracoContext.Current.Application.ApplicationCache.RequestCache.GetCacheItem<IEnumerable<string>>(
                "MultilingualUrlProvider-GetOtherUrls-" + id,
                () =>
                    {
                        var content = umbracoContext.ContentCache.GetById(id) as Master;

                        if (content != null)
                        {
                            var domains = ApplicationContext.Current.Services.DomainService.GetAll(true).OrderBy(x => x.Id).ToList();
                            if (domains.Count > 1)
                            {
                                var urls = new List<string>();

                                // Don't use umbracoContext.PublishedContentRequest.Culture because this code is also called in the backend.
                                var currentCulture = Thread.CurrentThread.CurrentCulture.ToString();

                                // Find the domain that's used in the GetUrl method.
                                var domain = UmbracoContext.Current.IsFrontEndUmbracoRequest
                                                 ? domains.First(x => x.LanguageIsoCode.InvariantEquals(currentCulture))
                                                 : domains.First();

                                // Remove the found domain because it's already used.
                                domains.Remove(domain);

                                if (content.DocumentTypeAlias.InvariantEquals(MainPage.ModelTypeAlias))
                                {
                                    // Return the domain if we're on the homepage because on that node we've added the domains.
                                    urls.AddRange(domains.Select(x => x.DomainName.EnsureEndsWith("/")));
                                }
                                else
                                {
                                    // Get the other urls for the parent which aren't the main url.
                                    if (content.Parent != null)
                                    {
                                        var parentUrls = umbracoContext.UrlProvider.GetOtherUrls(content.Parent.Id).ToList();
                                        for (int i = 0; i < domains.Count; i++)
                                        {
                                            // Get the domain and matching parent url.
                                            var otherDomain = domains[i];
                                            var otherParentUrl = parentUrls[i];
                                            // Get the parent url and add the url segment of this culture.
                                            var urlSegment = content.GetUrlSegment(otherDomain.LanguageIsoCode);
                                            urls.Add(otherParentUrl.EnsureEndsWith("/") + urlSegment);
                                        }
                                    }
                                    else
                                    {
                                        //string parentUrl = domain.
                                        for (int i = 0; i < domains.Count; i++)
                                        {
                                            // Get the domain and matching parent url.
                                            var otherDomain = domains[i];
                                            // Get the parent url and add the url segment of this culture.
                                            var urlSegment = content.GetUrlSegment(otherDomain.LanguageIsoCode);
                                            urls.Add(otherDomain.DomainName.EnsureEndsWith("/") + urlSegment);
                                        }
                                    }
                                }

                                return urls;
                            }
                        }

                        return null;
                    });
        }
    }
}