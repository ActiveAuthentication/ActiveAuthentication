using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveAuthenticationDesktopClient
{
    public class Similarity : Verifier
    {
        double sigmaFactor = 2.0; // SimilarityStDevScalar "CONFIGURATION WIREUP" (changed from SimilarityMaxDifferenceForValidMatch, default 2.0)
        int minMatchingPairs = 10;

        #region IVerifierModule Members
        public string VerifierType
        {
            get { return "Similarity"; }
        }

        public override void Verify(DataSet dsFeatures, DataSet dsFeaturesInliers, DataSet dsFeatureLabelGrouped, DataSet dsTemplate, DataSet dsVerifyResults)
        {
            DataTable dtFeaturesInliers = dsFeatures.Tables[0];
            DataTable dtTemplate = dsTemplate.Tables[0];

            try {
                var query = from featureValue in dtFeaturesInliers.AsEnumerable()
                            join featureStats in dtTemplate.AsEnumerable()
                            on new 
                            { 
                                ft = featureValue.Field<string>("FeatureType"), 
                                fl = featureValue.Field<string>("FeatureLabel") 
                            } equals new 
                            { 
                                ft = featureStats.Field<string>("FeatureType"), 
                                fl = featureStats.Field<string>("FeatureLabel") 
                            }
                            group new
                            {
                                SecurityId = featureValue.Field<string>("SecurityId"),
                                SampleId = featureValue.Field<string>("SampleId"),
                                FeatureType = featureValue.Field<string>("FeatureType"),
                                FeatureLabel = featureValue.Field<string>("FeatureLabel"),
                                FeatureValue = featureValue.Field<double>("FeatureValue"),
                                Mean = featureStats.Field<double>("Mean"),
                                StDev = featureStats.Field<double>("StDev")
                            } by featureValue["FeatureType"] into resultGroup
                            select new 
                            { 
                                resultGroup, 
                                SecurityId = resultGroup.FirstOrDefault().SecurityId, 
                                SampleId = resultGroup.FirstOrDefault().SampleId, 
                                FeatureType = resultGroup.FirstOrDefault().FeatureType,
                                Count = resultGroup.Count() 
                            };

                sigmaFactor = 2.0f; // Configuration.similarsimilarityMaxDifferenceForValidMatch;

                foreach (var g in query)
                {
                    double withinBoundary = 0;

                    foreach (var row in g.resultGroup)
                    {
                        if ((double)row.FeatureValue >= 0)
                        {
                            double upperLimit = row.Mean + sigmaFactor * row.StDev;
                            double lowerLimit = row.Mean - sigmaFactor * row.StDev;

                            if (lowerLimit <= row.FeatureValue && row.FeatureValue <= upperLimit)
                                withinBoundary++;
                        }
                    }

                    double score = withinBoundary / g.Count;

                    minMatchingPairs = Configuration.similarityMinMatchingPairs;

                    if (!Double.IsNaN(score) && !Double.IsInfinity(score) && g.Count >= minMatchingPairs) //SimilarityMinMatchingPairs "CONFIGURATION WIREUP"
                    dsVerifyResults.Tables[0].Rows.Add(g.SecurityId, g.SampleId, VerifierType, g.FeatureType, score);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Thrown in Verifier: {0}: {1}", VerifierType.ToString(), e.Message);
            }
        }
        #endregion
    }
}
