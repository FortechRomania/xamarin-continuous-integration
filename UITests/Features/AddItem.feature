Feature: Add Item
    I want to add new items to my list
	
Scenario: Add Item to List
	Given I tap on Add Item
	And I add and item using as name: 'Some Item' and as description: 'Some Description'
	When I tap Save Item
	Then I should see the item added as last item has name: 'Some Item' and description: 'Some Description' 