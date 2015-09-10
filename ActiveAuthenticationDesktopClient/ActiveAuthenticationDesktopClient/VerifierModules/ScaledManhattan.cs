using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveAuthenticationDesktopClient
{
    public class ScaledManhattan : Verifier
    {
        int reqMatchingPairs;

        #region IVerifierModule Members
        public string VerifierType
        {
            get { return "ScaledManhattan"; }
        }

        public override void Verify(DataSet dsFeatures, DataSet dsFeaturesInliers, DataSet dsFeatureLabelGrouped, DataSet dsTemplate, DataSet dsVerifyResults)
        {
            DataTable dtFeatureLabelGrouped = dsFeatureLabelGrouped.Tables[0];
            DataTable dtTemplate = dsTemplate.Tables[0];

            reqMatchingPairs = Configuration.scaledManhattanMinMatchingPairs;

            try
            {
                var query = from featureValue in dtFeatureLabelGrouped.AsEnumerable()
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
                                FeatureCount = featureStats.Field<int>("Count"), // For better accuracy, change this from featureValue to featureStats
                                Mean = featureStats.Field<double>("Mean"),
                                StDev = featureStats.Field<double>("StDev"),
                                AbsDev = featureStats.Field<double>("AbsDev")
                            } by featureValue["FeatureType"] into resultGroup
                            select new
                            {
                                resultGroup = resultGroup.OrderByDescending(x => x.FeatureCount),
                                SecurityId = resultGroup.FirstOrDefault().SecurityId,
                                SampleId = resultGroup.FirstOrDefault().SampleId,
                                FeatureType = resultGroup.FirstOrDefault().FeatureType,
                                Count = resultGroup.Count()
                            };

                // Iterate through each feature type.
                foreach (var g in query)
                {
                    double sumOfAbs = 0;
                    double countOfIncludedFeatures = 0;

                    // Iterate through each feature type value.
                    foreach (var row in g.resultGroup)
                    {
                        if (row.AbsDev + 1 > 0)
                        {
                            sumOfAbs += (Math.Abs(Convert.ToDouble(row.Mean) - Convert.ToDouble(row.FeatureValue)) + 1) / Convert.ToDouble(row.AbsDev + 1);
                            countOfIncludedFeatures++;
                        }

                        if (countOfIncludedFeatures >= reqMatchingPairs)
                            break;
                    }

                    double score = sumOfAbs;

                    // Save the score in the VerifyResults data set / data table.
                    if (!Double.IsNaN(score) && !Double.IsInfinity(score) && countOfIncludedFeatures == reqMatchingPairs)
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
