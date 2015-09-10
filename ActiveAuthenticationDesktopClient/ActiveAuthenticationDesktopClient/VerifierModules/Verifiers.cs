using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace ActiveAuthenticationDesktopClient
{
    class Verifiers
    {
        private static Verifier[] verifierModules;

        public static bool trainingUser = false;
        public static int trainedSamples = 0;

        public static int receivedSamples = 0;

        public static string currentUser;
        public static string currentSampleId;

        public static bool keyboardChange;
        public static string keyboardNameCaptured;
        public static string keyboardNameOnRecord;

        public static DataSet dsProfile = null;
        public static DataSet dsProfileFeatures = null;
        public static DataSet dsProfileFeaturesInliers = null;
        public static DataSet dsImposterFeatures = null;

        public static DataTable dtProfileFeatures = null;
        public static DataTable dtProfileFeaturesInliers = null;

        public static DataSet dsGroupedProfile = null;
        public static DataTable dtGroupedProfile = null;

        public static DataSet dsVerifierScores = null;
        public static DataTable dtVerifierScores = null;

        public static Dictionary<string, string> featureGroupLookup = new Dictionary<string, string>();

        public Verifiers()
        {
            dsVerifierScores = new DataSet();
            dtVerifierScores = new DataTable("Verifier_Message");

            dtVerifierScores.Columns.Add(new DataColumn("SecurityId", typeof(string)));
            dtVerifierScores.Columns.Add(new DataColumn("SampleId", typeof(string)));
            dtVerifierScores.Columns.Add(new DataColumn("VerifierType", typeof(string)));
            dtVerifierScores.Columns.Add(new DataColumn("FeatureType", typeof(string)));
            dtVerifierScores.Columns.Add(new DataColumn("VerifierFeatureScore", typeof(double)));

            dsVerifierScores.Tables.Add(dtVerifierScores);

            verifierModules = new Verifier[] {new Absolute(), new Relative(), new ScaledEuclidean(), new ScaledManhattan(), new Similarity()};
        }

        public DataSet RunVerifier(DataSet dsFeatures)
        {
            dsVerifierScores.Tables[0].Clear();
            DataTable dtFeatures = dsFeatures.Tables[0];
            //count the sample before processing
            receivedSamples++;

            if (dtFeatures.Rows.Count < 1)
                return dsVerifierScores;

            DataSet dsFeaturesInliers;

            // Get the current sample ID.
            GetCurrentSampleId(ref currentSampleId, dsFeatures);

            // Check for any change in the profile we should be using --N
            if (keyboardChange == true /*|| contextChange == true*/ || DetectUserChange(ref currentUser, dsFeatures) == true)
            {
                if (LoadProfileFromDisk(currentUser, out dsProfile))
                {
                    keyboardChange = false;
                   // contextChange = false;
                    trainingUser = false;
                }
                else if(LoadFromTrainingBuffer(currentUser, out dsProfileFeatures, out dsProfileFeaturesInliers, out trainedSamples))
                {
                    keyboardChange = false;
                    //contextChange = false;
                    trainingUser = true;
                }
                else
                {
                    // Training / Enrollment / Profile creation begins, clear the Profilefeatures table.
                    if (dtProfileFeatures != null)
                        dtProfileFeatures.Rows.Clear();
                    else
                    {
                        // Create the Profile features data set.  This has the outliers removed for Profile creation and
                        // contains the number of samples necessary for training / enrollment / Profile creation.
                        dsProfileFeatures = new DataSet();
                        dtProfileFeatures = new DataTable("ProfileFeatures");

                        dtProfileFeatures.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                        dtProfileFeatures.Columns.Add(new DataColumn("SampleId", typeof(string)));
                        dtProfileFeatures.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                        dtProfileFeatures.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                        dtProfileFeatures.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

                        dsProfileFeatures.Tables.Add(dtProfileFeatures);
                    }

                    if (dtProfileFeaturesInliers != null)
                        dtProfileFeaturesInliers.Rows.Clear();
                    else
                    {
                        // Create the Profile features data set.  This has the outliers removed for Profile creation and
                        // contains the number of samples necessary for training / enrollment / Profile creation.
                        dsProfileFeaturesInliers = new DataSet();
                        dtProfileFeaturesInliers = new DataTable("ProfileFeaturesInliers");

                        dtProfileFeaturesInliers.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                        dtProfileFeaturesInliers.Columns.Add(new DataColumn("SampleId", typeof(string)));
                        dtProfileFeaturesInliers.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                        dtProfileFeaturesInliers.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                        dtProfileFeaturesInliers.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

                        dsProfileFeaturesInliers.Tables.Add(dtProfileFeaturesInliers);
                    }


                    // Set the verifier in Profile training mode and reset the count of samples added
                    // to the current Profile.
                    keyboardChange = false;
                    //contextChange = false;
                    trainingUser = true;
                    trainedSamples = 0;
                    receivedSamples = 1;
                }
            }

            // Copy the features provided into the dsFeaturesInliers data set.
            dsFeaturesInliers = dsFeatures.Copy(); // If not copying, just get the features and put it in the inliers field.

            // Check for default table existence in dsFeaturesInliers and clear it (while retaining or creating the table def).
            if (dsFeaturesInliers.Tables != null && dsFeaturesInliers.Tables.Count > 0)
                dsFeaturesInliers.Tables[0].Rows.Clear();
            else
            {
                dsFeaturesInliers.Clear();
                dsFeaturesInliers.Tables.Add(dsFeatures.Tables[0].Clone());
            }

            // Import the rows from dsFeatures's into dsFeaturesInliers's default table.
            foreach (DataRow dr in dsFeatures.Tables[0].Rows)
                dsFeaturesInliers.Tables[0].ImportRow(dr);
            // Check to see if a user is currently being trained, and if the correct number of "Skipped [Micro] Samples" are skipped for aggregation
            if ((Configuration.useSlidingWindow && (trainingUser && (receivedSamples % Configuration.numSkippedSamples) == 0)) ||
                (!Configuration.useSlidingWindow && trainingUser))
            {
                // Ensure the system is collecting the same sample across messages to support multiple sources
                // and train only until we have enough (requiredTrainingSamples) samples.  Training is completed 
                // when a new SampleId is received and the count of different samples is equal to or greater than 
                // requiredTrainingSamples.  This does not support receiving a new SampleId and then messages for 
                // an old sample.  
                if (trainedSamples < Configuration.requiredTrainingSamples)
                {
                    // Collect samples for training / enrollment / Profile creation.
                    AppendRowsToDataSet(dsFeatures, ref dsProfileFeatures);
                    AppendRowsToDataSet(dsFeaturesInliers, ref dsProfileFeaturesInliers);

                    // Increment the count of samples collected for training.
                    trainedSamples++;
                    // save the sample to the buffer for this context and keyboard combination
                    SaveTrainingBuffer(dsProfileFeatures, dsProfileFeaturesInliers, trainedSamples);
                }
                else
                {
                    // Print out how many 
                    Console.WriteLine("Samples {0} and {1} KSEs collected in the training buffer.", trainedSamples, dtFeatures.Rows.Count);

                    // Calculate the statistics used for the Profile.
                    ComputeProfile(dsProfileFeatures, dsProfileFeaturesInliers, dsImposterFeatures, out dsProfile);

                    // Save the Profile to disk.
                    SaveProfileToDisk(dsProfile);

                    // Switch from training mode.
                    trainingUser = false;

                    // Training / Enrollment / Profile creation begins, clear the Profilefeatures table so they don't remain in memory.
                    dtProfileFeatures.Rows.Clear();
                    dtProfileFeaturesInliers.Rows.Clear();
                }
            }

            if (!trainingUser)// not training the user, verifying.
            {
                if (Configuration.featureGroupingSize > 1)
                {

                    if (dsGroupedProfile == null)
                    {
                        dsGroupedProfile = new DataSet();
                        dtGroupedProfile = new DataTable();

                        dtGroupedProfile.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                        dtGroupedProfile.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                        dtGroupedProfile.Columns.Add(new DataColumn("Count", typeof(int)));
                        dtGroupedProfile.Columns.Add(new DataColumn("Mean", typeof(double)));
                        dtGroupedProfile.Columns.Add(new DataColumn("StDev", typeof(double)));
                        dtGroupedProfile.Columns.Add(new DataColumn("AbsDev", typeof(double)));

                        dsGroupedProfile.Tables.Add(dtGroupedProfile);

                        var groupByFeatureType = from profile in dsProfile.Tables[0].AsEnumerable()
                                                 group profile by profile.Field<string>("FeatureType") into g
                                                 select new
                                                 {
                                                     Items = g.OrderBy(x => x.Field<double>("Mean"))
                                                              .Select((x, i) => new { Index = i / Configuration.featureGroupingSize, Item = x })
                                                              .GroupBy(x => x.Index, g2 => g2.Item).Select(
                                                              g2 => new
                                                              {
                                                                  FeatureType = g.Key,
                                                                  FeatureLabel = String.Join("$", g2.Select(x => x.Field<string>("FeatureLabel"))),
                                                                  Count = g2.Sum(x => x.Field<int>("Count")),
                                                                  SumOfX = g2.Sum(x => (double)x.Field<int>("Count") * x.Field<double>("Mean")),
                                                                  SumOfX2 = g2.Sum(x => (Math.Pow((double)x.Field<int>("Count") * x.Field<double>("Stdev"), 2.0) + Math.Pow(x.Field<double>("Mean") * (double)x.Field<int>("Count"), 2.0)) / (double)x.Field<int>("Count"))
                                                              }
                                                     )
                                                 };

                        featureGroupLookup.Clear();

                        foreach (var row in groupByFeatureType)
                        {
                            foreach (var row2 in row.Items)
                            {
                                string[] featureLabels = row2.FeatureLabel.Split('$');

                                foreach (string featureLabel in featureLabels)
                                    featureGroupLookup.Add(row2.FeatureType + "|" + featureLabel, row2.FeatureLabel);

                                // We are inserting StDev into the AbsDev column of the profile table.
                                // The only verifier that should be affected by this is Scaled Manhattan
                                dtGroupedProfile.Rows.Add(row2.FeatureType, row2.FeatureLabel, row2.Count, row2.SumOfX / (double)row2.Count, Math.Sqrt((row2.SumOfX2 * (double)row2.Count) - Math.Pow(row2.SumOfX, 2.0)) / (double)row2.Count, Math.Sqrt((row2.SumOfX2 * (double)row2.Count) - Math.Pow(row2.SumOfX, 2.0)) / (double)row2.Count);
                            }
                        }
                    }

                    DataSet dsGroupedFeatures = new DataSet();
                    DataSet dsGroupedFeatureInliers = new DataSet();

                    DataTable dtGroupedFeatures = new DataTable();

                    dtGroupedFeatures.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                    dtGroupedFeatures.Columns.Add(new DataColumn("SampleId", typeof(string)));
                    dtGroupedFeatures.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                    dtGroupedFeatures.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                    dtGroupedFeatures.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

                    dsGroupedFeatures.Tables.Add(dtGroupedFeatures);

                    foreach (DataRow row in dsFeatures.Tables[0].Rows)
                    {
                        string lookupString = (string)row["FeatureType"] + "|" + (string)row["FeatureLabel"];

                        dtGroupedFeatures.Rows.Add(row["SecurityId"], row["SampleId"], row["FeatureType"], (featureGroupLookup.ContainsKey(lookupString) ? featureGroupLookup[lookupString] : row["FeatureLabel"]), row["FeatureValue"]);
                    }

                    DataTable dtGroupedFeatureInliers = new DataTable();

                    dtGroupedFeatureInliers.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                    dtGroupedFeatureInliers.Columns.Add(new DataColumn("SampleId", typeof(string)));
                    dtGroupedFeatureInliers.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                    dtGroupedFeatureInliers.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                    dtGroupedFeatureInliers.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

                    dsGroupedFeatureInliers.Tables.Add(dtGroupedFeatureInliers);

                    foreach (DataRow row in dsFeaturesInliers.Tables[0].Rows)
                    {
                        string lookupString = (string)row["FeatureType"] + "|" + (string)row["FeatureLabel"];

                        dtGroupedFeatureInliers.Rows.Add(row["SecurityId"], row["SampleId"], row["FeatureType"], (featureGroupLookup.ContainsKey(lookupString) ? featureGroupLookup[lookupString] : row["FeatureLabel"]), row["FeatureValue"]);
                    }

                    var featureLabelGrouped = from features in dsGroupedFeatures.Tables[0].AsEnumerable()
                                              group features by new { FeatureType = features.Field<string>("FeatureType"), FeatureLabel = features.Field<string>("FeatureLabel") } into g
                                              select new
                                              {
                                                  SecurityId = g.First().Field<string>("SecurityId"),
                                                  SampleId = g.First().Field<string>("SampleId"),
                                                  FeatureType = g.Key.FeatureType,
                                                  FeatureLabel = g.Key.FeatureLabel,
                                                  FeatureValue = g.Average(x => x.Field<double>("FeatureValue")),
                                                  Count = g.Count()
                                              };

                    DataSet dsFeatureLabelGrouped = new DataSet();

                    DataTable dtFeatureLabelGrouped = new DataTable();

                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("SampleId", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("FeatureValue", typeof(double)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("Count", typeof(int)));

                    dsFeatureLabelGrouped.Tables.Add(dtFeatureLabelGrouped);

                    foreach (var row in featureLabelGrouped)
                    {
                        dtFeatureLabelGrouped.Rows.Add(row.SecurityId, row.SampleId, row.FeatureType, row.FeatureLabel, row.FeatureValue, row.Count);
                    }

                    // Call all verifier modules on dsFeatures.
                    foreach (Verifier v in verifierModules)
                    {
                        v.Verify(dsGroupedFeatures, dsGroupedFeatureInliers, dsFeatureLabelGrouped, dsGroupedProfile,dsVerifierScores);
                    }

                    
                }
                else
                {
                    var featureLabelGrouped = from features in dsFeatures.Tables[0].AsEnumerable()
                                              group features by new { FeatureType = features.Field<string>("FeatureType"), FeatureLabel = features.Field<string>("FeatureLabel") } into g
                                              select new
                                              {
                                                  SecurityId = g.First().Field<string>("SecurityId"),
                                                  SampleId = g.First().Field<string>("SampleId"),
                                                  FeatureType = g.Key.FeatureType,
                                                  FeatureLabel = g.Key.FeatureLabel,
                                                  FeatureValue = g.Average(x => x.Field<double>("FeatureValue")),
                                                  Count = g.Count()
                                              };

                    DataSet dsFeatureLabelGrouped = new DataSet();

                    DataTable dtFeatureLabelGrouped = new DataTable();

                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("SampleId", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("FeatureValue", typeof(double)));
                    dtFeatureLabelGrouped.Columns.Add(new DataColumn("Count", typeof(int)));

                    dsFeatureLabelGrouped.Tables.Add(dtFeatureLabelGrouped);

                    foreach (var row in featureLabelGrouped)
                    {
                        dtFeatureLabelGrouped.Rows.Add(row.SecurityId, row.SampleId, row.FeatureType, row.FeatureLabel, row.FeatureValue, row.Count);
                    }

                    // Call all verifier modules on dsFeatures.
                    foreach (Verifier v in verifierModules)
                    {
                        v.Verify(dsFeatures, dsFeaturesInliers, dsFeatureLabelGrouped, dsProfile, dsVerifierScores);
                    }
                    
                }
            }
            return dsVerifierScores;
        }

        private static bool DetectUserChange(ref string currentUser, DataSet dsFeatures)
        {
            DataTable dtFeatures;
            bool returnValue = false;

            if (dsFeatures != null && dsFeatures.Tables.Count > 0)
            {
                dtFeatures = dsFeatures.Tables[0];

                // Get the security ID from the last row of the features data table.  This is the "user" of the current sample.
                string latestUser = dtFeatures.Rows[dtFeatures.Rows.Count - 1]["SecurityId"].ToString();

                // Check to see if the "currentUser" member variable is the same as the latestUser or SecurityId login associated 
                // with the latest sample contained in dtFeatures.
                if (currentUser != latestUser)
                {
                    // Set the "currentUser" member variable to the latestUser or the SecurityId login associated with the 
                    // latest sample contained in dtFeatures.
                    currentUser = latestUser;
                    returnValue = true;
                }
            }

            return returnValue;
        }

        private static void RemoveOutliers(DataSet dsFeatures, out DataSet dsFeaturesInliers)
        {
            DataTable dtFeaturesInliers;
            DataTable dtFeatures;
            double r_distance = Configuration.outlierDetectionRadius;

            dsFeaturesInliers = new DataSet();

            // If dsFeatures is null or empty, there is nothing to do.  If dsFeaturesInliers is null, there is nowhere to return data.
            if (dsFeatures != null && dsFeatures.Tables.Count > 0)
            {
                // Create the features inliers data set.  This has the outliers removed.
                dtFeaturesInliers = new DataTable("FeaturesInliers");

                dtFeaturesInliers.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                dtFeaturesInliers.Columns.Add(new DataColumn("SampleId", typeof(string)));
                dtFeaturesInliers.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                dtFeaturesInliers.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                dtFeaturesInliers.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

                dsFeaturesInliers.Tables.Add(dtFeaturesInliers);

                // Create pointers to the datasets' primary tables.
                dtFeatures = dsFeatures.Tables[0];

                // Anchor at the ith feature row, and look for matching featurelabels at the jth row, and check if it is a neighbor.
                // If there are at least 68% neighbors compared to the number of total matches, then it is an inlier and save it.
                foreach (DataRow rowFeature in dtFeatures.Rows)
                {
                    int neighbors = 0;
                    int matchCount = 0;
                    double distance = 0;

                    string currentFeatureType = rowFeature["FeatureType"].ToString();
                    string currentFeatureLabel = rowFeature["FeatureLabel"].ToString();

                    DataRow[] rowsFilteredFeatures = dtFeatures.Select("FeatureType='" + currentFeatureType + "' AND FeatureLabel='" + currentFeatureLabel + "'");

                    foreach (DataRow rowFilteredFeature in rowsFilteredFeatures)
                    {
                        matchCount++;

                        distance = Math.Abs((double)rowFeature["FeatureValue"] - (double)rowFilteredFeature["FeatureValue"]);

                        if (distance <= r_distance)
                            neighbors++;
                    }

                    if (neighbors >= .68 * (double)matchCount)
                    {
                        // Copy the row from dtFeatures to dtFeaturesInliers.
                        dtFeaturesInliers.ImportRow(rowFeature);
                    }
                }
            }
        }

        private static bool LoadProfileFromDisk(string currentUser, out DataSet dsProfile)
        {
            AADesktopClient.unlockFiles();
            string ProfilePathFilename = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + @".xml";
            bool fileLoaded = false;
            DataTable dtProfile;

            // Create the Profile data set.
            dsProfile = new DataSet();

            //dsGroupedProfile = null;

            dtProfile = new DataTable("Profile");

            dtProfile.Columns.Add(new DataColumn("FeatureType", typeof(string)));
            dtProfile.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
            dtProfile.Columns.Add(new DataColumn("Count", typeof(int)));
            dtProfile.Columns.Add(new DataColumn("Mean", typeof(double)));
            dtProfile.Columns.Add(new DataColumn("StDev", typeof(double)));
            dtProfile.Columns.Add(new DataColumn("AbsDev", typeof(double)));

            dsProfile.Tables.Add(dtProfile);

            // Clear out the Profile data table.
            dtProfile.Rows.Clear();

            // Check to see if the Profile already exists in storage, load and return true.
            // If it does not exist, only return false.
            if (File.Exists(ProfilePathFilename))
            {
                // test to make sure the profile is within the 6 month window
               /* DateTime tolorance = System.DateTime.Now.AddMonths(-6);
                if (System.DateTime.Compare(tolorance, File.GetCreationTime(ProfilePathFilename)) <= 0)
                {*/
                // longitudinal data needed for the tolorance window --N
                dsProfile.ReadXml(ProfilePathFilename, XmlReadMode.ReadSchema);

                // Set the file load flag to report a successful Profile load.
                fileLoaded = true;
               /* }
                else
                    File.Delete(ProfilePathFilename);*/
            }

            // Return the file loaded status.  Default is false / bad load.
            AADesktopClient.lockFiles();
            return fileLoaded;
        }

        private static void SaveProfileToDisk(DataSet dsProfile)
        {
            string ProfilePathFilename = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + ".xml";
            AADesktopClient.unlockFiles();
            dsProfile.WriteXml(ProfilePathFilename);
            AADesktopClient.lockFiles();
        }

        private static void SaveTrainingBuffer(DataSet dsProfileFeatures, DataSet dsProfileFeaturesInliers, int SampleCount)
        {
            string ProfileFeaturesPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "ProfileFeaturesBuffer.xml";
            string FeaturesInliersPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "ProfileFeaturesInliersBuffer.xml";
            string sampleCountPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "SampleCount.txt";
            AADesktopClient.unlockFiles();
            dsProfileFeatures.WriteXml(ProfileFeaturesPath);
            dsProfileFeaturesInliers.WriteXml(FeaturesInliersPath);
            File.WriteAllText(sampleCountPath, SampleCount.ToString());
            AADesktopClient.lockFiles();
        }

        private static bool LoadFromTrainingBuffer(string currentUser, out DataSet dsProfileFeatures, out DataSet dsProfileFeaturesInliers, out int trainedSamples)
        {
            AADesktopClient.unlockFiles();
            string ProfileFeaturesPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "ProfileFeaturesBuffer.xml";
            string FeaturesInliersPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "ProfileFeaturesInliersBuffer.xml";
            string sampleCountPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "SampleCount.txt";
            bool fileLoaded = false;
            dsProfileFeatures = new DataSet();
            dtProfileFeatures = new DataTable("ProfileFeatures");

            dtProfileFeatures.Columns.Add(new DataColumn("SecurityId", typeof(string)));
            dtProfileFeatures.Columns.Add(new DataColumn("SampleId", typeof(string)));
            dtProfileFeatures.Columns.Add(new DataColumn("FeatureType", typeof(string)));
            dtProfileFeatures.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
            dtProfileFeatures.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

            dsProfileFeatures.Tables.Add(dtProfileFeatures);
            dtProfileFeatures.Rows.Clear();
            dsProfileFeaturesInliers = new DataSet();
            dtProfileFeaturesInliers = new DataTable("ProfileFeaturesInliers");

            dtProfileFeaturesInliers.Columns.Add(new DataColumn("SecurityId", typeof(string)));
            dtProfileFeaturesInliers.Columns.Add(new DataColumn("SampleId", typeof(string)));
            dtProfileFeaturesInliers.Columns.Add(new DataColumn("FeatureType", typeof(string)));
            dtProfileFeaturesInliers.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
            dtProfileFeaturesInliers.Columns.Add(new DataColumn("FeatureValue", typeof(double)));

            dsProfileFeaturesInliers.Tables.Add(dtProfileFeaturesInliers);
            dtProfileFeaturesInliers.Rows.Clear();
            int Samples = 0;
            if(File.Exists(ProfileFeaturesPath) && File.Exists(FeaturesInliersPath) && File.Exists(sampleCountPath))
            {
                // test to make sure the buffers are within the 6 month window
               /* DateTime tolorance = System.DateTime.Now.AddMonths(-6);
                if ((System.DateTime.Compare(tolorance, File.GetCreationTime(ProfileFeaturesPath)) <= 0) && (System.DateTime.Compare(tolorance, File.GetCreationTime(FeaturesInliersPath)) <=0))
                {*/
                // above and below sections are commented out until further longitudnal data is collected --N
                dsProfileFeatures.ReadXml(ProfileFeaturesPath, XmlReadMode.ReadSchema);
                dsProfileFeaturesInliers.ReadXml(FeaturesInliersPath, XmlReadMode.ReadSchema);

                Samples = Convert.ToInt16(File.ReadAllText(sampleCountPath));
                // Set the file load flag to report a successful Profile load.
                fileLoaded = true;
               /* }
                else
                {
                    File.Delete(ProfileFeaturesPath);
                    File.Delete(FeaturesInliersPath);
                    File.Delete(sampleCountPath);
                    Samples = 0;
                }*/
            }
            else if((File.Exists(ProfileFeaturesPath)) && !(File.Exists(FeaturesInliersPath)))
            {
                File.Delete(ProfileFeaturesPath);
                if (File.Exists(sampleCountPath))
                    File.Delete(sampleCountPath);
                Samples = 0;
            }
            else if(!(File.Exists(ProfileFeaturesPath)) && (File.Exists(FeaturesInliersPath)))
            {
                File.Delete(FeaturesInliersPath);
                if (File.Exists(sampleCountPath))
                    File.Delete(sampleCountPath);
                Samples = 0;
            }
            trainedSamples = Samples;
            AADesktopClient.lockFiles();
            return fileLoaded;
        }

        private static void GetCurrentSampleId(ref string currentSampleId, DataSet dsFeatures)
        {
            if (dsFeatures.Tables.Count > 0)
            {
                DataTable dtFeatures = dsFeatures.Tables[0];

                currentSampleId = dtFeatures.Rows[dtFeatures.Rows.Count - 1]["SampleId"].ToString();
            }
        }

        private static void ComputeProfile(DataSet dsProfileFeatures, DataSet dsProfileFeaturesInliers, DataSet dsImposterFeatures, out DataSet dsProfile)
        {
            DataTable dtProfileFeaturesInliers;
            DataTable dtProfile;

            // Must set dsProfile as it is an out parameter.
            dsProfile = new DataSet();


            if (dsProfileFeaturesInliers.Tables.Count > 0)
            {
                dtProfileFeaturesInliers = dsProfileFeaturesInliers.Tables[0];

                // Create the Profile data set.
                dtProfile = new DataTable("Profile");

                dtProfile.Columns.Add(new DataColumn("FeatureType", typeof(string)));
                dtProfile.Columns.Add(new DataColumn("FeatureLabel", typeof(string)));
                dtProfile.Columns.Add(new DataColumn("Count", typeof(int)));
                dtProfile.Columns.Add(new DataColumn("Mean", typeof(double)));
                dtProfile.Columns.Add(new DataColumn("StDev", typeof(double)));
                dtProfile.Columns.Add(new DataColumn("AbsDev", typeof(double)));

                dsProfile.Tables.Add(dtProfile);

                if (dtProfileFeaturesInliers.Rows.Count > 0)
                {
                    // Compute mean and count grouped by all of the feature types and feature labels, put it in an IEnumerable list.
                    var rowsCountMean =
                        from table in dtProfileFeaturesInliers.AsEnumerable()
                        group table by new { FeatureType = table["FeatureType"], FeatureLabel = table["FeatureLabel"] } into g
                        orderby g.Key.FeatureType, g.Key.FeatureLabel
                        select new
                        {
                            FeatureType = g.Key.FeatureType,
                            FeatureLabel = g.Key.FeatureLabel,
                            Count = (double)g.Count(),
                            Mean = g.Average(x => x.Field<double>("FeatureValue"))
                        };

                    // Iterate through the generated list of rows to process for standard deviation.
                    foreach (var row in rowsCountMean)
                    {
                        // Retrieve only the rows with the current FeatureType and FeatureLabel in them.
                        DataRow[] rowsFilteredTypeLabel = dtProfileFeaturesInliers.Select("FeatureType='" + row.FeatureType + "' AND FeatureLabel='" + row.FeatureLabel + "'");
                        double sumOfDiffSquared = 0;
                        double sumOfAbsDiff = 0;

                        // Calculate the stddev.  This is NOT a rolling calculation as has been done in the past by the CIC, because continual Profile updates are 
                        // not expected on this project.  However, SumOfXSquared is still being stored in the Profile in case rolling calculation is desired.  
                        // The mathematical proof of why this O(n^2) method is chosen over the O(n) method is provided on the TFS server under Documents\8_Resources\Proofs.
                        // Also calculate the absolute deviation using sumOfAbsDiff.
                        foreach (DataRow rowFilteredTypeLabel in rowsFilteredTypeLabel)
                        {
                            double diff = row.Mean - Convert.ToDouble(rowFilteredTypeLabel["FeatureValue"]);

                            sumOfAbsDiff += Math.Abs(diff);
                            sumOfDiffSquared += diff * diff;
                        }

                        double absdev = sumOfAbsDiff / (row.Count);
                        double stdev = Math.Sqrt(sumOfDiffSquared / (row.Count)); //using population standard deviation for a test. It was originally population stdev

                        // Insert the featureLabel, count, mean, standard deviation, and absolute deviation into the Profile.
                        // AARON UPDATE WITH MINIMUM OF 4 ROW.COUNT!
                        if (rowsFilteredTypeLabel.Length >= Configuration.templateMinFeatureCount)
                            dtProfile.Rows.Add(row.FeatureType, row.FeatureLabel, row.Count, row.Mean, stdev, absdev);
                    }
                }

                //// Compute the automatic thresholds based off of the user's training data, the user's typing data, 
                //// and the imposter's stored typing data (currently pulls from the database).
                //ComputeThresholds(dsProfileFeatures, dsProfileFeaturesInliers, dsImposterFeatures, ref dsProfile);
            }
            string ProfileFeaturesPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "ProfileFeaturesBuffer.xml";
            string FeaturesInliersPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "ProfileFeaturesInliersBuffer.xml";
            string sampleCountPath = Configuration.profilePath + @"\" + currentUser + keyboardNameCaptured /*+ contextCaptured*/ + "SampleCount.txt";
            AADesktopClient.unlockFiles();
            if(File.Exists(ProfileFeaturesPath))
            {
                File.Delete(ProfileFeaturesPath);
            }
            if(File.Exists(FeaturesInliersPath))
            {
                File.Delete(FeaturesInliersPath);
            }
            if(File.Exists(sampleCountPath))
            {
                File.Delete(sampleCountPath);
            }
            AADesktopClient.lockFiles();
        }

        private static void AppendRowsToDataSet(DataSet dsSource, ref DataSet dsDestination)
        {
            DataTable dtSource;
            DataTable dtDestination;

            // Only attempt to append rows to dsDestination if there are rows to append from the source in dsSource.
            if (dsSource != null && dsSource.Tables.Count > 0 && dsSource.Tables[0].Rows.Count > 0)
            {
                dtSource = dsSource.Tables[0];

                if (dsDestination == null || dsDestination.Tables.Count == 0)
                    dsDestination = dsSource.Copy(); // If dsDestination is null or its default table is non-existent, just copy the source.
                else
                {
                    // If dsDestination is not null and has a default table, just append source rows to it.
                    dtDestination = dsDestination.Tables[0];

                    for (int i = 0; i < dtSource.Rows.Count; i++)
                        dtDestination.ImportRow(dtSource.Rows[i]);
                }
            }
        }

    }
}
