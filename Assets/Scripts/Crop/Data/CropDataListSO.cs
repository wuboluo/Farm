using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CropDataListSO", menuName = "Crop/CropDataList")]

public class CropDataListSO : ScriptableObject
{
    public List<CropDetails> cropDetailsList;
}
