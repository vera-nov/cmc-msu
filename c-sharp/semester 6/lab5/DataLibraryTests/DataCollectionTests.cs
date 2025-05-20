using ClassLibrary;

namespace DataLibraryTests
{
    public class DataCollectionTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithTwoItems()
        {
            var dataCollection = new DataCollection();
            Assert.Equal(2, dataCollection.Obs.Count);
        }

        [Fact]
        public void Add_ShouldIncreaseCollectionCount()
        {
            var dataCollection = new DataCollection();
            int initialCount = dataCollection.Obs.Count;

            var newItem = new DataItem(5);
            dataCollection.Add(newItem);

            Assert.Equal(initialCount + 1, dataCollection.Obs.Count);
            Assert.Same(newItem, dataCollection.Obs[^1]);
        }

        [Fact]
        public void UpdateDataCollection_ShouldUpdateAndReplaceItems()
        {
            var dataCollection = new DataCollection();
            var originalSecondItem = dataCollection.Obs[1];

            dataCollection.UpdateDataCollection();

            Assert.Equal("UpdatedUser", dataCollection.Obs[0].Name);
            Assert.NotSame(originalSecondItem, dataCollection.Obs[1]);
            Assert.Equal(3, dataCollection.Obs.Count);
            Assert.Equal("User8", dataCollection.Obs[2].Name);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            var dataCollection = new DataCollection();
            string result = dataCollection.ToString();

            Assert.True(result.Contains("User1"));
            Assert.True(result.Contains("User2"));
            Assert.True(result.Contains("("));
        }
    }

    public class DataItemTests
    {
        [Fact]
        public void DefaultConstructor_SetsExpectedDefaults()
        {
            var item = new DataItem();
            Assert.Equal("PrivilegedUser", item.Name);
            Assert.Equal(("Flower", 10), item.SITuple);
        }

        [Fact]
        public void ParameterizedConstructor_SetsExpectedValues()
        {
            var j = 3;
            var item = new DataItem(j);

            Assert.Equal("User3", item.Name);
            Assert.Equal(new DateTime(2023, 4, 4), item.Date);
            Assert.Equal(("Type9", 84), item.SITuple);
            Assert.Equal(24, item.LowerBound);
            Assert.Equal(60, item.UpperBound);
        }

        [Fact]
        public void PropertyChanged_IsRaisedOnNameChange()
        {
            var item = new DataItem();
            bool eventRaised = false;

            item.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Name")
                    eventRaised = true;
            };

            item.Name = "NewName";

            Assert.True(eventRaised);
        }

        [Fact]
        public void PropertyChanged_IsRaisedOnUpperBoundChange()
        {
            var item = new DataItem();
            int count = 0;

            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "UpperBound" || e.PropertyName == "LowerBound")
                    count++;
            };

            item.UpperBound = 999;

            Assert.Equal(2, count);
        }

        [Fact]
        public void IDataErrorInfo_ValidatesBoundsCorrectly()
        {
            var item = new DataItem
            {
                LowerBound = 100,
                UpperBound = 50
            };

            Assert.Equal("LowerBound must be less than UpperBound", item[nameof(item.LowerBound)]);
            Assert.Equal("UpperBound must be more than LowerBound", item[nameof(item.UpperBound)]);
        }

        [Fact]
        public void IDataErrorInfo_ValidatesDateCorrectly()
        {
            var item = new DataItem { Date = new DateTime(2035, 1, 1) };

            Assert.Equal("Year must be less than 2030", item[nameof(item.Date)]);
        }

        [Fact]
        public void ToString_ReturnsExpectedFormat()
        {
            var item = new DataItem(2);
            var expected = $"User2 {item.Date.ToShortDateString()} {item.SITuple.ToString()} {item.LowerBound} {item.UpperBound}";

            Assert.Equal(expected, item.ToString());
        }

        [Fact]
        public void CreateLongTimeDataItem_CreatesCorrectly()
        {
            var start = DateTime.Now;
            var item = DataItem.CreateLongTimeDataItem(1, 100);
            var elapsed = DateTime.Now - start;

            Assert.Equal("User1", item.Name);
            Assert.True(elapsed.TotalMilliseconds >= 100);
        }
    }
}