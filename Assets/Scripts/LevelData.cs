using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData {

	public int MonsterIndex;
	public int DiamondsRequired;

	public LevelData(int monsterIndex, int diamondsReq)
	{
		MonsterIndex = monsterIndex;
		DiamondsRequired = diamondsReq;
	}
}
