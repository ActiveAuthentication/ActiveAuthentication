using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ActiveAuthenticationDesktopClient.FeatureExtractors
{
    class FeatureExtractor_KH_Next : FeatureExtractor
    {
        public FeatureExtractor_KH_Next(FeatureExtractorArguments fea) : base (fea)
        {}

        public string FeatureType
        {
            get { return "KeyHoldWithNextVKCode"; /* KeyHoldWithNextVKCode */ }
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

                if (dtCollection.Rows.Count > 1)
                {
                    int[] isInHold = new int[96]; // 96 codes
                    for (int i = 0; i < isInHold.Length; i++)
                    {
                        isInHold[i] = 0;
                    }

                    int[] lastValidPress = new int[96]; // 96 codes
                    for (int i = 0; i < lastValidPress.Length; i++)
                    {
                        lastValidPress[i] = -1;
                    }

                    // Iterate through rows
                    for (int i = 0; i < dtCollection.Rows.Count - 1; i++)
                    {
                        int vkCode = Convert.ToInt16(dtCollection.Rows[i]["VkCode"]);
                        if (vkCode >= 32 && vkCode <= 127) // 96 codes
                        {
                            int charIndex = vkCode - 32; // 96 codes
                            if (Convert.ToInt16(dtCollection.Rows[i]["KeyEvent"]) == 1)
                            {
                                // Check if a press event for that key already has fired before its release (hold toggle)
                                if (isInHold[charIndex] != 1)
                                {
                                    // Flip the hold toggle and save the index where the press was recorded
                                    lastValidPress[charIndex] = i;
                                    isInHold[charIndex] = 1;
                                }
                            }
                            else
                            {
                                // Make sure there is an associated key press event to this release
                                if (lastValidPress[charIndex] >= 0)
                                {
                                    // Calculate KeyHoldWithNextVKCode value and store in dtFeatures

                                    // Calculate KeyHoldWithNextVKCode time based on difference from release timestamp and its associated press timestamp
                                    long KeyHoldWithNextVKCode = Convert.ToInt64(double.Parse(dtCollection.Rows[i]["AbsoluteTimestamp"].ToString()) - double.Parse(dtCollection.Rows[lastValidPress[charIndex]]["AbsoluteTimestamp"].ToString()));

                                    string featureLabel = dtCollection.Rows[i]["VkCode"].ToString() + '|' + dtCollection.Rows[i + 1]["VkCode"].ToString();
                                    dtFeatures.Rows.Add(securityId, sampleId, FeatureType, featureLabel, KeyHoldWithNextVKCode);


                                    // Flip the hold toggle back to 0 (next press for that key is valid)
                                    isInHold[charIndex] = 0;
                                    lastValidPress[charIndex] = -1;
                                }
                            }
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
