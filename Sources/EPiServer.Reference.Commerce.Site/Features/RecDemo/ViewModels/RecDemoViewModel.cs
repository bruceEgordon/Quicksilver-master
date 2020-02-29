using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.RecDemo.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.RecDemo.ViewModels
{
    public class RecDemoViewModel
    {
        public string CodeValue { get; set; }
        public RecDemoPage RecDemoPage { get; set; }
        public IEnumerable<RecommendedProduct> HomeRecommendations { get; set; }
        public IEnumerable<RecommendedProduct> ProductAltRecommendations { get; set; }
        public IEnumerable<RecommendedProduct> ProductCrossSellRecommendations { get; set; }
    }

    public class RecommendedProduct
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string UnitPrice { get; set; }
        public string SalePrice { get; set; }
        public string Code { get; set; }
        public long recommendationId { get; set; }
    }
}