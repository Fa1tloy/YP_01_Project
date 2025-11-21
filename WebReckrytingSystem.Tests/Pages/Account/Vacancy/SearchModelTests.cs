using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Pages.Vacancy;
using WebReckrytingSystem.Services;

namespace UnitTests.Pages.Vacancy
{
    [TestClass]
    public class SearchModelTests
    {
        private readonly Mock<IVacancySearchService> _mockVacancySearchService;
        private readonly Mock<ILogger<SearchModel>> _mockLogger;
        private readonly SearchModel _searchModel;

        public SearchModelTests()
        {
            _mockVacancySearchService = new Mock<IVacancySearchService>();
            _mockLogger = new Mock<ILogger<SearchModel>>();

            _searchModel = new SearchModel(_mockVacancySearchService.Object, _mockLogger.Object)
            {
                PageContext = new PageContext(),
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
        }

        [TestMethod]
        public async Task OnGetAsync_WithSearchParameters_CallsSearchService()
        {
            // Arrange
            _searchModel.SearchData = new SearchVacancyViewModel { Keywords = ".NET" };
            var searchResult = new SearchResult<Vacancy>
            {
                Items = new List<Vacancy> { new Vacancy { Title = "Senior .NET Developer" } },
                TotalCount = 1
            };

            _mockVacancySearchService.Setup(x => x.SearchVacancies(It.IsAny<SearchVacancyViewModel>()))
                .Returns(ServiceResult<SearchResult<Vacancy>>.Success("Найдено 1 вакансий", searchResult));

            // Act
            var result = await _searchModel.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.AreEqual("Найдено 1 вакансий", _searchModel.SuccessMessage);
            Assert.AreEqual(1, _searchModel.SearchResult.Items.Count);
            _mockVacancySearchService.Verify(x => x.SearchVacancies(It.IsAny<SearchVacancyViewModel>()), Times.Once);
        }

        [TestMethod]
        public async Task OnGetAsync_WithoutSearchParameters_DoesNotCallSearchService()
        {
            // Arrange
            _searchModel.SearchData = new SearchVacancyViewModel(); // Пустые параметры

            // Act
            var result = await _searchModel.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            _mockVacancySearchService.Verify(x => x.SearchVacancies(It.IsAny<SearchVacancyViewModel>()), Times.Never);
        }

        [TestMethod]
        public void OnPost_WithValidModel_RedirectsToGetWithQueryString()
        {
            // Arrange
            _searchModel.SearchData = new SearchVacancyViewModel
            {
                Keywords = ".NET Developer",
                SalaryFrom = 100000,
                Page = 1,
                PageSize = 10
            };

            _searchModel.ModelState.Clear();

            // Act
            var result = _searchModel.OnPost();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            var redirectResult = (RedirectResult)result;
            Assert.IsTrue(redirectResult.Url.Contains("Keywords=.NET%20Developer"));
            Assert.IsTrue(redirectResult.Url.Contains("SalaryFrom=100000"));
        }

        [TestMethod]
        public void OnPost_WithInvalidModel_ReturnsPage()
        {
            // Arrange
            _searchModel.SearchData = new SearchVacancyViewModel();
            _searchModel.ModelState.AddModelError("Keywords", "Required");

            // Act
            var result = _searchModel.OnPost();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
    }
}