using Xamarin.UITest;

using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace Jenkins.UITests.Pages
{
    public class ItemsListPage : BasePage
    {
        private Query AddItemButton => (e) => e.Marked(IsOnIos ? "Add Item" : "Add");

        private Query MoreOptions => (e) => e.Marked("More options");

        public int NumberOfItems => App.Query((e) => e.Class(IsOnIos ? "UITableViewCell" : "CardView")).Length;

        public void TapAddItem() {
            if (IsOnIos) 
            {
                App.Tap(AddItemButton);
            }
            else 
            {
                App.Tap(MoreOptions);
                App.Tap(AddItemButton);
            }
        } 

        public void TapItemAtIndex(int index) 
        {
            App.Tap(ItemRow(index));
        }

        public string NameAtIndex(int index)
        {
            return App.Query((e) => e.Marked(IsOnIos ? "ItemRowTitle" : "item_row_title"))[index].Text;
        }

        public string DescriptionAtIndex(int index)
        {
            return App.Query((e) => e.Marked(IsOnIos ? "ItemRowDescription" : "item_row_description"))[index].Text;
        }

        private Query ItemRow(int index) => (e) => e.Class(IsOnIos ? "UITableViewCell" : "CardView").Index(index);
    }
}
