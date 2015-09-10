using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ActiveAuthenticationDesktopClient.FeatureExtractors
{
    class FeatureExtractor_KRL : FeatureExtractor
    {
        public FeatureExtractor_KRL(FeatureExtractorArguments fea) : base (fea)
        {}

        public string FeatureType
        {
            get { return "KRL"; }
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

                List<int> KP_char = new List<int>();
                List<long> KP_times = new List<long>();
                List<int> KR_char = new List<int>();
                List<long> KR_times = new List<long>();

                int KP_index = 0;
                int KR_index = 0;

                // The number of times/frequency a keypair occured
                int[,] KRL_individual_counter = new int[96, 96]; // 96 codes

                // Import each row of the dsCollection table into either the key release (KP) lists, or the key release (KR) lists.
                foreach (DataRow row in dtCollection.Rows)
                {
                    int vkCode = Convert.ToInt32(row["VkCode"]);
                    long absoluteTimestamp = Convert.ToInt64(double.Parse(row["AbsoluteTimestamp"].ToString()));

                    if (Convert.ToInt16(row["KeyEvent"]) == 1) // release
                    {

                        // 96 codes
                        if (vkCode >= 32 && vkCode <= 127) // Only compute KPL off of VkCodes 65-90 or VkCodes 'A' - 'Z' and 'a' - 'z'.
                            KP_char.Add(vkCode - 32); // Subtract the first VkCode of concern, 'A' or 'a', which is vkCode 65.
                        else
                            KP_char.Add(1001); // Add a dummy value, 1001, to help identify non-acceptable keystroke events without 
                        // creating misalignments between KP_char and KP_times.

                        KP_times.Add(Convert.ToInt64(double.Parse(row["AbsoluteTimestamp"].ToString())));

                        KP_index++;
                    }
                    else // release
                    {

                        // 96 codes
                        if (vkCode >= 32 && vkCode <= 127)
                            KR_char.Add(vkCode - 32);
                        else
                            KR_char.Add(1001);

                        KR_times.Add(Convert.ToInt64(double.Parse(row["AbsoluteTimestamp"].ToString())));

                        KR_index++;
                    }
                }

                // Extract KRLs /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // Now KR_index is the number of events + 1
                // Should be just the number of events, not events + 1
                // Iterate through KR events
                for (int i = 0; i < Math.Min(KR_index - 1, KP_index - 1); i++) // ERROR IN ORIGINAL CODE, OUT OF RANGE EXCEPTION, HAD TO SUBTRACT ONE FROM INDEX COUNTER
                {
                    // The characters for this pair
                    int index1 = KR_char[i];
                    int index2 = KR_char[i + 1];

                    // Skip non 65-90 vkCodes.
                    if (index1 == 1001 || index2 == 1001) // Not a pair
                        continue;

                    // Check for repeated characters
                    if (index1 == index2)
                    {
                        int press_count = 0;
                        int release_count = 0;

                        for (int j = 0; j < KP_index - 1; j++) // Since KP and KR can be differently sized, only scan to the minimal count value.
                            if (KP_char[j] == KP_char[i])
                                press_count++;

                        for (int j = 0; j < KR_index - 1; j++) // Since KP and KR can be differently sized, only scan to the minimal count value.
                            if (KR_char[j] == KP_char[i])
                                release_count++;

                        // If there are an unequal number of releasees to releases (someone held down a key and sent a lot of release events), skip / continue.
                        if (press_count != release_count)
                            continue;
                    }

                    // Key release latency = absolute time of the second release - absolute time of the first release
                    int krl = (int)(KR_times[i + 1] - KR_times[i]);

                    if (KRL_individual_counter[index1, index2] < 200 && krl >= 0) // 200 ms maximum digram timing
                    {
                        string featureLabel = (index1 + 32).ToString() + "|" + (index2 + 32).ToString(); // 96 codes

                        // Add the feature type with feature label and the feature type value to the dtFeatures.
                        dtFeatures.Rows.Add(securityId, sampleId, FeatureType, featureLabel, krl);

                        // Adds one to the value at the index, still not sure why, possibly to prevent divide by 0 errors
                        KRL_individual_counter[index1, index2]++;
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
