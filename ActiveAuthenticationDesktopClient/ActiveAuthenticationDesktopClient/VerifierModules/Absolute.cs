using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveAuthenticationDesktopClient
{
    public class Absolute : Verifier
    {
        double tAbsolute = 1.45; // AbsoluteMaxRatioForValidMatch "CONFIGURATION WIREUP" (default 1.45)
        int minMatchingPairs = 10;


        #region IVerifierModule Members
        public string VerifierType
        {
            get { return "Absolute"; }
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
                                Mean = featureStats.Field<double>("Mean")
                            } by featureValue["FeatureType"] into resultGroup
                            select new
                            {
                                resultGroup,
                                SecurityId = resultGroup.FirstOrDefault().SecurityId,
                                SampleId = resultGroup.FirstOrDefault().SampleId,
                                FeatureType = resultGroup.FirstOrDefault().FeatureType,
                                Count = resultGroup.Count()
                            };

                foreach (var g in query)
                {

                    var query2 = from featureRows in g.resultGroup
                                 group featureRows by featureRows.FeatureLabel into featureLabels
                                 orderby featureLabels.Key
                                 select new
                                 {
                                     TemplateMean = featureLabels.Average(templateMean => templateMean.Mean),
                                     FeatureMean = featureLabels.Average(featureValue => featureValue.FeatureValue),
                                     FeatureLabel = featureLabels.Key
                                 };

                    int numUniquePairs = 0, numSimiliarPairs = 0;

                    // Get the mean value of Feature Values per Feature Type
                    foreach (var row in query2)
                    {
                        double minLatency = 0, maxLatency = 0;

                        // Find the minimum and maximum values between the template mean and feature value
                        if (row.TemplateMean >= row.FeatureMean)
                        {
                            maxLatency = row.TemplateMean;
                            minLatency = row.FeatureMean;
                        }
                        else
                        {
                            maxLatency = row.FeatureMean;
                            minLatency = row.TemplateMean;
                        }

                        // Ensure we won't get a 0 or divide by 0 in similarityValue calculation
                        if (maxLatency <= 0 || minLatency <= 0)
                            continue;

                        double similarityValue = maxLatency / minLatency;

                        numUniquePairs++;

                        tAbsolute = Configuration.absoluteMaxRatioForValidMatch;
                        minMatchingPairs = Configuration.absoluteMinMatchingPairs;

                        // Check if similarity value falls within proper range
                        if (similarityValue >= 1.0 && similarityValue <= tAbsolute)
                            numSimiliarPairs++;
                    }

                    // Create final score for Feature
                    double score = (double) 1.0 - (double) numSimiliarPairs / (double) numUniquePairs;
                    score = Math.Round(score, 6);

                    if (!Double.IsNaN(score) && !Double.IsInfinity(score) && numUniquePairs >= minMatchingPairs) //AbsoluteMinimumMatchingPairs "CONFIGURATION WIREUP" (default 10)
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