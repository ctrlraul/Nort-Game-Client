using System.Collections.Generic;

namespace Nort;

public class MissionCompletion
{
	public float Time { get; }
	public float Score { get; }
	public IEnumerable<PartData> PartsCollected { get; }

	public MissionCompletion(float time, float score, IEnumerable<PartData> partsCollected)
	{
		Time = time;
		Score = score;
		PartsCollected = partsCollected;
	}
}