using Xamarin.UITest;

using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace Jenkins.UITests.Pages
{
    public class AddItemPage : BasePage
    {
        private Query NameTextField => (e) => e.Marked(IsOnIos ? "Item Name" : "txtTitle");

        private Query DescriptionTextField => (e) => e.Marked(IsOnIos ? "Item Description" : "txtDesc");

        private Query SaveButton => (e) => e.Marked(IsOnIos ? "Save Item" : "save_button");

        public void TapSave() => App.Tap(SaveButton);

        public void PopulateFields(string name, string description) 
        {
            App.EnterText(NameTextField, name);
            App.EnterText(DescriptionTextField, description);
        }
    }
}
