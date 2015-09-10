using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ActiveAuthenticationDesktopClient.FeatureExtractors;

namespace ActiveAuthenticationDesktopClient
{
	class IdentityVerifier
	{
		public DataTable KeyboardEvents;
        public DataSet dsKeyboardEvents;
		private FeatureExtractorArguments FEargs;
		private FeatureExtractor[] extractors;
		private Verifiers verifiers;
        public static Double TrustValue;
		public const int TRUST_RATE = 7500;
		public const int UNTRUST_RATE = 7500;
		public const int TRUST_THRESHOLD = 50;
		public int sampleCount = 0;
		public int microSamplesSent = 0;
		public int recievedSamples = 0;
		#if Recording
		private DataTable KElog;
		private DataSet KELOG;
		#endif
		public IdentityVerifier()
		{
			TrustValue = 70;

			#if Recording
			KELOG = new DataSet();
			KElog = new DataTable();
			KElog.Columns.Add(new DataColumn("SecurityId", typeof(string)));
			KElog.Columns.Add(new DataColumn("SampleId", typeof(string)));
			KElog.Columns.Add(new DataColumn("KeyEvent", typeof(int)));
			KElog.Columns.Add(new DataColumn("VkCode", typeof(ushort)));
			KElog.Columns.Add(new DataColumn("AbsoluteTimestamp", typeof(long)));
			KELOG.Tables.Add(KElog);
			#endif

			//Initialize the Keyboard Event table
            dsKeyboardEvents = new DataSet();
			KeyboardEvents = new DataTable();
			KeyboardEvents.Columns.Add(new DataColumn("SecurityId", typeof(string)));
			KeyboardEvents.Columns.Add(new DataColumn("SampleId", typeof(string)));
			KeyboardEvents.Columns.Add(new DataColumn("KeyEvent", typeof(int)));
			KeyboardEvents.Columns.Add(new DataColumn("VkCode", typeof(ushort)));
			KeyboardEvents.Columns.Add(new DataColumn("AbsoluteTimestamp", typeof(long)));
            dsKeyboardEvents.Tables.Add(KeyboardEvents);
			//Initialize feature extractor arguments
			FEargs = new FeatureExtractorArguments();
			//Initialize the feature extractors
			extractors = new FeatureExtractor[]{new FeatureExtractor_IK(FEargs), new FeatureExtractor_KH(FEargs),
												new FeatureExtractor_KH_Next(FEargs), new FeatureExtractor_KH_Prev(FEargs),
												new FeatureExtractor_KPL(FEargs), new FeatureExtractor_KRL(FEargs)};
			verifiers = new Verifiers();
		}
		/// <summary>
		/// This method collects keystrokes and does magic to verify your identity!
		/// </summary>
		/// <param name="args">This is the KeyboardHookEventArgs that contains all info about the KSE</param>
		public double KeyEvent(KeyboardHookEventArgs args)
		{
			//Store the keyboardevents in the keyboard event table
			KeyboardEvents.Rows.Add(args.SID, "", args.KeyEvent, args.VkCode, (args.KeyEventTime-621355968000000000) / 10000);
			#if Recording
			KElog.Rows.Add(args.SID, "", args.KeyEvent, args.VkCode, (args.KeyEventTime - 621355968000000000) / 10000);
			#endif
            

			//Check if a full sample has been collected and if so ship it to the feature extractors
			if (KeyboardEvents.Rows.Count > 0 && KeyboardEvents.Rows.Count % Configuration.slidingWindowJump == 0)//if enough events have been collected for a microsample
			{
				#if Recording
				KELOG.WriteXml(Path.GetPathRoot(Environment.SystemDirectory)+@"Users\Azriel\Desktop\NEWKahona");
#endif
                //Get and assign the sampleID to each row
				string sampleId = Guid.NewGuid().ToString();
				foreach (DataRow row in KeyboardEvents.Rows)
					row["SampleId"] = sampleId;

				#region FeatureExtractors
				//Create a dataset to store the feature extractor data
				DataSet extracted = new DataSet();
				extracted.Clear();
				foreach (FeatureExtractor fe in extractors)
				{
					//Run Feature Extractors
					extracted.Tables.Add(fe.extract(KeyboardEvents));
				}
				//Create a table to hold the total feature extractor data
				DataTable features = new DataTable();
				foreach (DataTable dt in extracted.Tables)
				{
					//Combine feature extractor tables
					features.Merge(dt);
				}

				microSamplesSent++;

				// Increment the sample counter to output how many samples have been sent when this user completes.
				if (microSamplesSent >= Configuration.numSkippedSamples)
				{
					sampleCount++;
					microSamplesSent = 0;
				}

				if (KeyboardEvents.Rows.Count >= Configuration.keyEventsPerSample)
				{
					//Slide the window down
					for (int i = 0; i < Configuration.slidingWindowJump; i++)
					{
						KeyboardEvents.Rows.RemoveAt(0);
					}
				}
				DataSet dsfeatures = new DataSet();
				dsfeatures.Tables.Add(features);
				#endregion
				//Send the feature extractor scores into the verifiers
				DataSet verifierScores = verifiers.RunVerifier(dsfeatures);

				if (verifierScores.Tables[0].Rows.Count>0)
				{
					#if OutputScores
                    //verifierScores.WriteXml(Path.GetPathRoot(Environment.SystemDirectory)+@"Users\Azriel\Desktop\Scores\VerifierScores\" + sampleCount.ToString() + microSamplesSent.ToString() + "scores.xml"); this appears to be here for verification purposes --N
					#endif
					//Send the verifier scores into the fuser
					DataTable Scores = Fuse_MajorityVoting(verifierScores);
					updateTrust((double)Scores.Rows[0]["Score"], (double)Scores.Rows[0]["Threshold"]);
					#if DEBUG
					Console.WriteLine("Score = " + ((double)Scores.Rows[0]["Score"]).ToString());
					Console.WriteLine("Trust = " + TrustValue.ToString());
					#endif
                    return TrustValue;
				}
			}
            return TrustValue;
		}
		#if Kahona
		/// <summary>
		/// This method collects keystrokes in the form of DataRows and does magic to verify your identity!
		/// </summary>
		/// <param name="args">This is the DataRow that contains all info about the KSE</param>
		public double FileEvent(DataRow args)
		{
			//Store the keyboardevents in the keyboard event table
			KeyboardEvents.Rows.Add(args["SecurityID"], "", args["KeyEvent"], args["VKCode"], args["AbsoluteTimestamp"]);

			//Check if a full sample has been collected and if so ship it to the feature extractors
			if (KeyboardEvents.Rows.Count > 0 && KeyboardEvents.Rows.Count % Configuration.slidingWindowJump == 0)//if enough events have been collected for a microsample
			{
				//Get and assign the sampleID to each row
				string sampleId = Guid.NewGuid().ToString();
				foreach (DataRow row in KeyboardEvents.Rows)
					row["SampleId"] = sampleId;

				#region FeatureExtractors
				//Create a dataset to store the feature extractor data
				DataSet extracted = new DataSet();
				foreach (FeatureExtractor fe in extractors)
				{
					//Run Feature Extractors
					extracted.Tables.Add(fe.extract(KeyboardEvents));
				}
				//Create a table to hold the total feature extractor data
				DataTable features = new DataTable();
				foreach (DataTable dt in extracted.Tables)
				{
					//Combine feature extractor tables
					features.Merge(dt);
				}

				microSamplesSent++;

				// Increment the sample counter to output how many samples have been sent when this user completes.
				if (microSamplesSent >= Configuration.numSkippedSamples)
				{
					sampleCount++;
					microSamplesSent = 0;
				}

				if (KeyboardEvents.Rows.Count >= Configuration.keyEventsPerSample)
				{
					//Slide the window down
					for (int i = 0; i < Configuration.slidingWindowJump; i++)
					{
						KeyboardEvents.Rows.RemoveAt(0);
					}
				}

				DataSet dsfeatures = new DataSet();
				dsfeatures.Tables.Add(features);
				#endregion
				DataSet verifierScores = verifiers.RunVerifier(dsfeatures);
				if (verifierScores.Tables[0].Rows.Count > 0)
				{
					#if OutputScores
					verifierScores.WriteXml(Path.GetPathRoot(Environment.SystemDirectory)+@"Users\Azriel\Desktop\Scores\VerifierScores\" + sampleCount.ToString() + microSamplesSent.ToString() + "scores.xml");
#endif
					DataTable Scores = Fuse_MajorityVoting(verifierScores);
					updateTrust((double)Scores.Rows[0]["Score"], (double)Scores.Rows[0]["Threshold"]);
#if DEBUG
					Console.WriteLine("Score = " + ((double)Scores.Rows[0]["Score"]).ToString());
					Console.WriteLine("Trust = " + TrustValue.ToString());
#endif
					return TrustValue;
				}
			}
			return TrustValue;
		}
#endif
        /// <summary>
		/// This method performs fusion on the verifier scores and returns the final score
		/// </summary>
		/// <param name="dsFeatureVerifierScores">A dataSet that contains the verifier scores</param>
		/// <returns>A dataTable that contains the score and threshold</returns>
		public DataTable Fuse_MajorityVoting(DataSet dsFeatureVerifierScores)
		{
			try
			{
				DataTable dtFeatureVerifierScores = dsFeatureVerifierScores.Tables[0];
				DataTable dtScores = new DataTable();

				dtScores.Columns.Add(new DataColumn("SecurityId", typeof(string)));
				dtScores.Columns.Add(new DataColumn("SampleId", typeof(string)));
				dtScores.Columns.Add(new DataColumn("Score", typeof(double)));
				dtScores.Columns.Add(new DataColumn("Threshold", typeof(double)));

				string strSecurityId = dtFeatureVerifierScores.Rows[0]["SecurityId"].ToString();
				string strSampleId = dtFeatureVerifierScores.Rows[0]["SampleId"].ToString();
				string strProfilePathFilename = Configuration.profilePath + @"\" + strSecurityId + @".xml";

				int totalVotes = 0, affirmativeVotes = 0;

				// Use statically generated thresholds
				DataSet dsThresholds = new DataSet();
				dsThresholds.ReadXml(Configuration.verifierThresholdsFile);
				DataTable dtThresholds = dsThresholds.Tables[0];

				var result = from Threshold in dtThresholds.AsEnumerable()
							 join Score in dtFeatureVerifierScores.AsEnumerable()
							 on new { VerifierType = Threshold.Field<string>("VerifierType"), FeatureType = Threshold.Field<string>("FeatureType") }
							 equals new { VerifierType = Score.Field<string>("VerifierType"), FeatureType = Score.Field<string>("FeatureType") }
							 select new { Score, Threshold };

				foreach (var row in result)
				{
					totalVotes++;
					if (Convert.ToBoolean(row.Threshold.Field<string>("GenuineIsZero")))
					{
						double d = row.Score.Field<double>("VerifierFeatureScore");
						double dd = Convert.ToDouble(row.Threshold.Field<string>("Threshold"));
						if (row.Score.Field<double>("VerifierFeatureScore") < Convert.ToDouble(row.Threshold.Field<string>("Threshold")))
						{
							affirmativeVotes++;
						}
					}
					else
					{
						if (row.Score.Field<double>("VerifierFeatureScore") > Convert.ToDouble(row.Threshold.Field<string>("Threshold")))
						{
							affirmativeVotes++;
						}
					}
				}

				double totalScore = (totalVotes > 0) ? (double)affirmativeVotes / (double)totalVotes : 1;

				dsThresholds = new DataSet();

				dsThresholds.ReadXml(Configuration.fuserThresholdsFile);

				dtThresholds = dsThresholds.Tables[0];

				// Take the data from the Sensor and collect it in the feature extractor table.
				dtScores.Rows.Add(strSecurityId, strSampleId, totalScore, dtThresholds.Rows[0]["Threshold"]);
				return dtScores;
			}
			catch (Exception e)
			{
				string FuserType = "MajorityVoting";
				System.Console.WriteLine("Exception Thrown in Fuser: {0}: {1}", FuserType.ToString(), e.Message);
				return null;
			}
		}
        /// <summary>
        /// Update the overall trust score
        /// </summary>
        /// <param name="score"></param>
        /// <param name="threshold"></param>
		private void updateTrust(double score, double threshold)
		{
			if (score<threshold)
			{
				TrustValue = Math.Max(TrustValue - (threshold - score) / (100 * (threshold) / UNTRUST_RATE), 0);
			}
			else
			{
				TrustValue = Math.Min(TrustValue + (score - threshold) / (100 * (1 - threshold) / TRUST_RATE), 100);
			}
		}
        /// <summary>
        /// if the keyboard in use is changed and Active Authentication is still in training mode save the
        /// training buffer so the user does not have to retrain the profile from scratch
        /// </summary>
        /// <param name="keyboardCaptured"></param>
        public void changeKeyboard(string keyboardCaptured)
        {
            if (!dsKeyboardEvents.Tables.Contains(keyboardCaptured /*+ Verifiers.contextCaptured*/))
            {
                KeyboardEvents = new DataTable(keyboardCaptured /*+ Verifiers.contextCaptured*/);
                KeyboardEvents.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                KeyboardEvents.Columns.Add(new DataColumn("SampleId", typeof(string)));
                KeyboardEvents.Columns.Add(new DataColumn("KeyEvent", typeof(int)));
                KeyboardEvents.Columns.Add(new DataColumn("VkCode", typeof(ushort)));
                KeyboardEvents.Columns.Add(new DataColumn("AbsoluteTimestamp", typeof(long)));
                dsKeyboardEvents.Tables.Add(KeyboardEvents);
            }
            else
                KeyboardEvents = dsKeyboardEvents.Tables[keyboardCaptured /*+ Verifiers.contextCaptured*/];
        }
        /// <summary>
        /// if the context in use is changed (though we are not currently capturing or using context switching) 
        /// and Active Authentication is still in training mode save the training buffer so the user does not
        /// have to retrain the profile from scratch
        /// </summary>
        /// <param name="contextCaptured"></param>
        public void changeContext(string contextCaptured)
        {
            if (!dsKeyboardEvents.Tables.Contains(Verifiers.keyboardNameCaptured + contextCaptured))
            {
                KeyboardEvents = new DataTable(Verifiers.keyboardNameCaptured + contextCaptured);
                KeyboardEvents.Columns.Add(new DataColumn("SecurityId", typeof(string)));
                KeyboardEvents.Columns.Add(new DataColumn("SampleId", typeof(string)));
                KeyboardEvents.Columns.Add(new DataColumn("KeyEvent", typeof(int)));
                KeyboardEvents.Columns.Add(new DataColumn("VkCode", typeof(ushort)));
                KeyboardEvents.Columns.Add(new DataColumn("AbsoluteTimestamp", typeof(long)));
                dsKeyboardEvents.Tables.Add(KeyboardEvents);
            }
            else
                KeyboardEvents = dsKeyboardEvents.Tables[Verifiers.keyboardNameCaptured + contextCaptured];
        }
	}
}
