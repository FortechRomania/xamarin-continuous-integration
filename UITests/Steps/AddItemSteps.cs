using Jenkins.UITests.Pages;
using SpecNuts;
using TechTalk.SpecFlow;

namespace Jenkins.UITests.Steps
{
    [Binding]
    public class AddItemSteps : ReportingStepDefinitions
    {
        [Given(@"I add and item using as name: '(.*)' and as description: '(.*)'")]
        public void AddItemWithTitleAndDescription(string name, string description)
        {
            var addItemPage = new AddItemPage();
            addItemPage.PopulateFields(name, description);
        }

        [When(@"I tap Save Item")]
        public void TapSaveItem()
        {
            var addItemPage = new AddItemPage();
            addItemPage.TapSave();
        }
    }
}
