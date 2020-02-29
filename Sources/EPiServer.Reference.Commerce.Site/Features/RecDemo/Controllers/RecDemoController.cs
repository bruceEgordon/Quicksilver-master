using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.RecDemo.Pages;
using EPiServer.Reference.Commerce.Site.Features.RecDemo.ViewModels;
using EPiServer.Tracking.Commerce;
using EPiServer.Web.Mvc;
using EPiServer.Personalization.Commerce.Extensions;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using EPiServer.Globalization;
using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using System;
using EPiServer.Commerce.Marketing;
using EPiServer.Tracking.Core;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Tracking.Commerce.Data;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Customers;

namespace EPiServer.Reference.Commerce.Site.Features.RecDemo.Controllers
{
    public class RecDemoController : PageController<RecDemoPage>
    {
private LanguageResolver _languageResolver;
private ICurrentMarket _currentMarket;
private IContentLoader _contentLoader;
private AssetUrlResolver _assetUrlResolver;
private IPriceService _priceService;
private IPromotionEngine _promotionEngine;
private ITrackingService _trackingService;
private TrackingDataFactory _trackingDataFactory;
private ReferenceConverter _referenceConverter;
private IOrderRepository _orderRepository;
private IOrderGroupFactory _orderGroupFactory;

        public RecDemoController(LanguageResolver languageResolver, ICurrentMarket currentMarket, IContentLoader contentLoader, AssetUrlResolver assetUrlResolver, IPriceService priceService, IPromotionEngine promotionEngine, ITrackingService trackingService, TrackingDataFactory trackingDataFactory, ReferenceConverter referenceConverter, IOrderRepository orderRepository, IOrderGroupFactory orderGroupFactory)
        {
            _languageResolver = languageResolver;
            _currentMarket = currentMarket;
            _contentLoader = contentLoader;
            _assetUrlResolver = assetUrlResolver;
            _priceService = priceService;
            _promotionEngine = promotionEngine;
            _trackingService = trackingService;
            _trackingDataFactory = trackingDataFactory;
            _referenceConverter = referenceConverter;
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
        }

        [CommerceTracking(TrackingType.Home)]
        public ActionResult Index(RecDemoPage currentPage)
        {
            var viewModel = new RecDemoViewModel();
            viewModel.RecDemoPage = currentPage;

            var recs = this.GetRecommendationGroups()
                .Where(x => x.Area == "homeWidget")
                .SelectMany(x => x.Recommendations);

            viewModel.HomeRecommendations = FillRecsModel(recs);

            return View(viewModel);
        }

        public PartialViewResult ProductRecommendations(string code, long recommendationId)
        {
            var viewModel = new RecDemoViewModel();
            viewModel.CodeValue = code;

            var trackData = _trackingDataFactory.CreateProductTrackingData(code, recommendationId, HttpContext);

            var prodRef = _referenceConverter.GetContentLink(code);
            var prod = _contentLoader.Get<EntryContentBase>(prodRef);

            var data = _trackingService.Track(trackData, HttpContext, prod);

            var prodAlts = data.GetRecommendationGroups()
                .Where(x => x.Area == "productAlternativesWidget")
                .SelectMany(x => x.Recommendations);

            viewModel.ProductAltRecommendations = FillRecsModel(prodAlts);

            var prodCrossSell = data.GetRecommendationGroups()
                .Where(x => x.Area == "productCrossSellsWidget")
                .SelectMany(x => x.Recommendations);

            viewModel.ProductCrossSellRecommendations = FillRecsModel(prodCrossSell);

            return PartialView(viewModel);
        }

        public ActionResult FakeCart(string code, long recommendationId)
        {
            var cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "Default");
            var prodRef = _referenceConverter.GetContentLink(code);
            var prod = _contentLoader.Get<ProductContent>(prodRef);
            var varRef = prod.GetVariants().FirstOrDefault();
            var varContent = _contentLoader.Get<VariationContent>(varRef);

            var lineItem = _orderGroupFactory.CreateLineItem(varContent.Code, cart);
            lineItem.PlacedPrice = _priceService.GetDefaultPrice(_currentMarket.GetCurrentMarket().MarketId, DateTime.Now, new CatalogKey(varContent.Code), _currentMarket.GetCurrentMarket().DefaultCurrency).UnitPrice;
            lineItem.Quantity = 1;
            cart.AddLineItem(lineItem);
            _orderRepository.Save(cart);

            ViewBag.ProdCode = code;
            ViewBag.RecId = recommendationId;
            return View();
        }

        private IEnumerable<RecommendedProduct> FillRecsModel(IEnumerable<Recommendation> recommendations)
        {
            var recs = new List<RecommendedProduct>();
            var catRefs = recommendations.Select(s => s.ContentLink);
            var catItems = _contentLoader.GetItems(catRefs, _languageResolver.GetPreferredCulture());
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currentMarket.GetCurrentMarket().DefaultCurrency;
            foreach(var catItem in catItems)
            {
                var product = catItem as ProductContent;
                var variantRef = product.GetVariants().FirstOrDefault();
                var variant = _contentLoader.Get<VariationContent>(variantRef);
                var price = _priceService.GetDefaultPrice(market.MarketId, DateTime.Now, new CatalogKey(variant.Code), currency);
                var discountPrice = _promotionEngine.GetDiscountPrices(variantRef, market, currency).FirstOrDefault()?.DiscountPrices.FirstOrDefault();
                var recProd = new RecommendedProduct
                {
                    Title = product.DisplayName,
                    UnitPrice = price?.UnitPrice.ToString(),
                    SalePrice = discountPrice?.Price.ToString(),
                    ImageUrl = _assetUrlResolver.GetAssetUrl(product),
                    Code = product.Code,
                    recommendationId = recommendations.Where(r => r.ContentLink == product.ContentLink).FirstOrDefault().RecommendationId
                };
                recs.Add(recProd);
            }
            return recs;
        }
    }
}