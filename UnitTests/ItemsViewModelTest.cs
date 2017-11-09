using Jenkins;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ItemsViewModelTest
    {
        [Test]
        public void Title_Is_Browse()
        {
            // Given
            var itemsViewModel = new ItemsViewModel();

            // When & Then
            Assert.AreEqual("Browse", itemsViewModel.Title);
        }

        [Test]
        public void LoadItems_ExpectedNumberOfItems()
        {
            // Given
            var itemsViewModel = new ItemsViewModel();

            // When
            itemsViewModel.LoadItemsCommand.Execute(null);

            // Then
            Assert.AreEqual(6, itemsViewModel.Items.Count);
        }
    }
}
