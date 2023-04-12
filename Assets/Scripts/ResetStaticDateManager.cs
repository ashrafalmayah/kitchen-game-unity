using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStaticDateManager : MonoBehaviour
{
    private void Awake() {
        BaseCounter.ResetStaticDate();
        CuttingCounter.ResetStaticDate();
        TrashCounter.ResetStaticDate();
        Player.ResetStaticDate();
    }
}
