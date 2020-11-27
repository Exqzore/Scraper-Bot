namespace Telegram_Bot
{
    class SpecificUserDatabaseAndRequests : UserDatabase
    {
        private bool _isSearch = false;

        private bool _flagRequestEditItemName = false;
        private bool _flagRequestEditItemPrice = false;
        private bool _flagRequestAddItemName = false;
        private bool _flagRequestAddItemPrice = false;

        private string _nameElementClick = null;
        private short _priceElementClick = 0;

        private string _inputItemName = null;
        private short _inputItemPrice = short.MaxValue;

        public SpecificUserDatabaseAndRequests(long chatId): base(chatId) {}

        public bool FlagRequestEditItemName
        {
            get { return _flagRequestEditItemName; }
            set { _flagRequestEditItemName = value; }
        }

        public bool FlagRequestEditItemPrice
        {
            get { return _flagRequestEditItemPrice; }
            set { _flagRequestEditItemPrice = value; }
        }

        public bool FlagRequestAddItemName
        {
            get { return _flagRequestAddItemName; }
            set { _flagRequestAddItemName = value; }
        }

        public bool FlagRequestAddItemPrice
        {
            get { return _flagRequestAddItemPrice; }
            set { _flagRequestAddItemPrice = value; }
        }

        public string NameElementClick
        {
            get { return _nameElementClick; }
            set { _nameElementClick = value; }
        }

        public string InputItemName
        {
            get { return _inputItemName; }
            set { _inputItemName = value; }
        }

        public short PriceElementClick
        {
            get { return _priceElementClick; }
            set { _priceElementClick = value; }
        }

        public short InputItemPrice
        {
            get { return _inputItemPrice; }
            set { _inputItemPrice = value; }
        }

        public bool IsSearch
        {
            get { return _isSearch; }
            set { _isSearch = value; }
        }

        public long ChatId
        {
            get { return _chatId; }
        }

        public void ResetRequest()
        {
            FlagRequestEditItemName = false;
            FlagRequestEditItemPrice = false;
            FlagRequestAddItemName = false;
            FlagRequestAddItemPrice = false;

            NameElementClick = null;
            PriceElementClick = 0;

            InputItemName = null;
            InputItemPrice = short.MaxValue;
        }
    }
}
