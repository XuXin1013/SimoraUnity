using UnityEngine;

namespace SimoraUnity.Activation
{
    public class License
    {
        public string rightProduct;
        public string product;
        public string version;
        public string device;
        public int startAt;//
        public int endAt;
        public string scopes;
        public string issue;
        public string extra;
        public int issueAt;//颁发时间

        public bool isValid()
        {
            if (product != rightProduct)
            {
                return false;
            }

            if (device != SystemInfo.deviceUniqueIdentifier)
            {
                return false;
            }

            return true;
        }
    }
}