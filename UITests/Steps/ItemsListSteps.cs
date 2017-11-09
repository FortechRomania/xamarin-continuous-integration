using Jenkins.UITests.Helpers;
using Jenkins.UITests.Pages;
using NUnit.Framework;
using SpecNuts;
using TechTalk.SpecFlow;

namespace Jenkins.UITests.Steps
{
    [Binding]
    public class ItemsListSteps : ReportingStepDefinitions
    {
        [Given(@"I tap on Add Item")]
        public void TapOnAddItem()
        {
            var itemsListPage = new ItemsListPage();
            itemsListPage.TapAddItem();
        }

        [Then(@"I should see the item added as last item has name: '(.*)' and description: '(.*)'")]
        public void SeeTheItemAddedAsLastItemOnItemsList(string name, string description)
        {
            var itemsListPage = new ItemsListPage();

            if (!AppManager.IsOnIos)
            {
                AppManager.App.ScrollDownByDragging();
            }

            var numberOfItems = itemsListPage.NumberOfItems;

            var actualName = itemsListPage.NameAtIndex(numberOfItems - 1);
            var actualDescription = itemsListPage.DescriptionAtIndex(numberOfItems - 1);

            Assert.AreEqual(name, actualName);
            Assert.AreEqual(description, actualDescription);
        }
    }
}
