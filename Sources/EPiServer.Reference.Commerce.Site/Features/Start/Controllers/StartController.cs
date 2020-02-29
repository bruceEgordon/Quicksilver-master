using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.ViewModels;
using EPiServer.Web.Mvc;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Personalization.Commerce;
using EPiServer.Personalization.Commerce.Widgets;
using EPiServer.Tracking.Core;
using EPiServer.Tracking.Commerce;
using System;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Extensions;

namespace EPiServer.Reference.Commerce.Site.Features.Start.Controllers
{
    public class StartController : PageController<StartPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICurrentMarket _currentMarket;
        private readonly MarketContentLoader _marketContentFilter;
        private readonly PersonalizationClientConfiguration _personalizationClientConfig;
        private readonly WidgetService _widgetService;
        private readonly IEnumerable<ITrackingDataInterceptor> _trackingDataInterceptors;

        public StartController(
            IContentLoader contentLoader,
            ICurrentMarket currentMarket,
            MarketContentLoader marketContentFilter,
            PersonalizationClientConfiguration personalizationClientConfiguration,
            WidgetService widgetService,
            IEnumerable<ITrackingDataInterceptor> trackingDataInterceptors)
        {
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
            _marketContentFilter = marketContentFilter;
            _personalizationClientConfig = personalizationClientConfiguration;
            _widgetService = widgetService;
            _trackingDataInterceptors = trackingDataInterceptors;
        }

        [CommerceTracking(TrackingType.Home)]
        public ViewResult Index(StartPage currentPage)
        {
            var viewModel = new StartPageViewModel()
            {
                StartPage = currentPage,
                Promotions = GetActivePromotions()
            };

            var widgets = Enumerable.Empty<Tuple<string, string, string, bool>>();

            foreach (var scope in _personalizationClientConfig.GetScopes())
            {
                WidgetsResponseData response = _widgetService.GetWidgets(scope);
                foreach (var page in response.EpiPerPage.Pages)
                {
                    widgets = widgets.Concat(page.Widgets.Select(w =>Tuple.Create(scope, page.PageType, w.WidgetName, w.Active)));
                }
            }

            ViewData["widgets"] = widgets;
            ViewData["interceptors"] = _trackingDataInterceptors.OrderBy(item => item.SortOrder);

            IEnumerable<Recommendation> recs = this.GetHomeRecommendations().Take(6);
            ViewData["recs"] = recs;

            return View(viewModel);
        }

        private IEnumerable<PromotionViewModel> GetActivePromotions()
        {
            var promotions = new List<PromotionViewModel>();

            var promotionItemGroups = _marketContentFilter.GetPromotionItemsForMarket(_currentMarket.GetCurrentMarket()).GroupBy(x => x.Promotion);

            foreach (var promotionGroup in promotionItemGroups)
            {
                var promotionItems = promotionGroup.First();
                promotions.Add(new PromotionViewModel()
                {
                    Name = promotionGroup.Key.Name,
                    BannerImage = promotionGroup.Key.Banner,
                    SelectionType = promotionItems.Condition.Type,
                    Items = GetProductsForPromotion(promotionItems).Take(3)
                });
            }

            return promotions;
        }

        private IEnumerable<CatalogContentBase> GetProductsForPromotion(PromotionItems itemsOnPromotion)
        {
            var conditionProducts = new List<CatalogContentBase>();

            foreach (var conditionItemReference in itemsOnPromotion.Condition.Items)
            {
                CatalogContentBase conditionItem;
                if (_contentLoader.TryGet(conditionItemReference, out conditionItem))
                {
                    AddIfProduct(conditionItem, conditionProducts);
                    var nodeContent = conditionItem as NodeContentBase;
                    if (nodeContent != null)
                    {
                        AddItemsRecursive(nodeContent, itemsOnPromotion, conditionProducts);
                    }
                }
            }

            return conditionProducts;
        }

        private void AddItemsRecursive(NodeContentBase nodeContent, PromotionItems itemsOnPromotion, List<CatalogContentBase> conditionProducts)
        {
            foreach (var child in _contentLoader.GetChildren<CatalogContentBase>(nodeContent.ContentLink))
            {
                AddIfProduct(child, conditionProducts);

                var childNode = child as NodeContentBase;
                if (childNode != null && itemsOnPromotion.Condition.IncludesSubcategories)
                {
                    AddItemsRecursive(childNode, itemsOnPromotion, conditionProducts);
                }
            }
        }

        private static void AddIfProduct(CatalogContentBase content, List<CatalogContentBase> productsInPromotion)
        {
            if (content is ProductContent)
            {
                productsInPromotion.Add(content);
            }
        }
    }
}