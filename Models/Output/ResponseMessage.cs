using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Models.Output
{
    public class ResponseMessage
    {
        private Types _tipeEnum;
        private IEnumerable<string> _messages;

        public enum Types
        {
            None = 0,
            Information = 1,
            Warning = 2,
            Error = 3,
            ForceExit = 4
        }

        public ResponseMessage()
        {
            _tipeEnum = Types.Information;
        }

        [IgnoreDataMember]
        public Types TypeEnum
        {
            set { _tipeEnum = value; }
            get { return _tipeEnum; }
        }        

        [DataMember]
        public string TransationsNumber { get; set; }

        [DataMember]
        public ICollection<string> Messages
        {
            get { return (ICollection<string>)(_messages ?? (_messages = new List<string>())); }
            set { _messages = value; }
        }

        public string SerializeMessage
        {
            get { return string.Join("|", Messages); }
        }
    }
}
