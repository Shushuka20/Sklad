using System;

namespace Sklad.Services
{
    public class ClaimService
    {
        private string editCode = "&/edit/&";

        public string AddStateForEditProperty(string _value)
        {
            if (_value != null)
                return editCode + _value;
            return "";
        }

        public bool ReadStateProperty(string _value)
        {
            if (_value != null)
                return _value.Contains(editCode);

            return false;
        }

        public string ReadProperty(string _value)
        {
            if (_value != null)
                return _value.Replace(editCode, "");
            return "";
        }
    }
}