using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace ActiveAuthenticationDesktopClient.FeatureExtractors
{
    class FeatureExtractor_IK : FeatureExtractor
    {
        public FeatureExtractor_IK(FeatureExtractorArguments fea) : base (fea)
        {}

        public string FeatureType
        {
            get { return "IK"; }
        }

        public override DataTable extract(DataTable KeyboardEvents)
        {
            try
            {
                DataTable dtFeatures = new DataTable();
                dtFeatures.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                dtFeatures.Columns.Add(new DataColumn("SampleId", typeof(string)));
                dtFeatures.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                dtFeatures.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                dtFeatures.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

                DataTable dtCollection = KeyboardEvents;

                // Get the first row's SampleId and SecurityId.  This assumes a sample only contains one user.  Awaiting a response from Novetta to ensure this is the case.
                string sampleId = dtCollection.Rows[0].Field<string>("SampleId");
                string securityId = dtCollection.Rows[0].Field<string>("SecurityId");

                if (dtCollection.Rows.Count > 3)
                {

                    int lastValidPress = -1, lastValidRelease = -1, nextValidPress = -1;

                    for (int i = 0; i < dtCollection.Rows.Count; i++)
                    {
                        DataRow row = dtCollection.Rows[i];

                        int vkCode = Convert.ToInt16(row["VkCode"]);

                        if (Convert.ToInt16(row["KeyEvent"]) == 1)
                        {
                            if (lastValidPress < 0)
                            {
                                lastValidPress = i;
                            }
                            else if (nextValidPress < 0)
                            {
                                if (lastValidRelease >= 0 || vkCode != Convert.ToInt16(dtCollection.Rows[lastValidPress]["VkCode"]))
                                {
                                    nextValidPress = i;
                                }
                            }
                        }
                        else
                        {
                            if (lastValidPress >= 0 && vkCode == Convert.ToInt16(dtCollection.Rows[lastValidPress]["VkCode"]) && lastValidRelease < 0)
                            {
                                lastValidRelease = i;
                            }
                        }
                        if (lastValidRelease >= 0 && nextValidPress >= 0 && lastValidPress >= 0)
                        {
                            int firstVk = Convert.ToInt16(dtCollection.Rows[lastValidPress]["VkCode"]);
                            int secondVk = Convert.ToInt16(dtCollection.Rows[nextValidPress]["VkCode"]);

                            if (firstVk >= 32 && firstVk <= 127 && secondVk >= 32 && secondVk <= 127) // 96 codes
                            {
                                // Calculate keyhold value and store in dtFeatures
                                string featureLabel = (firstVk).ToString() + "|" + (secondVk).ToString(); // 96 codes

                                long intervalKey = Convert.ToInt64(double.Parse(dtCollection.Rows[nextValidPress]["AbsoluteTimestamp"].ToString()) - double.Parse(dtCollection.Rows[lastValidRelease]["AbsoluteTimestamp"].ToString()));

                                dtFeatures.Rows.Add(securityId, sampleId, FeatureType, featureLabel, intervalKey);
                            }

                            i = nextValidPress;
                            lastValidPress = i;
                            nextValidPress = -1;
                            lastValidRelease = -1;
                        }
                    }
                }
                return dtFeatures;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Thrown in Feature Extractor: {0}: {1}", FeatureType.ToString(), e.Message);
            }

            return null;
        }
    }
}
