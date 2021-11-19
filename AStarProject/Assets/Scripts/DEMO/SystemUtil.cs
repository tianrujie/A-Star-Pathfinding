using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SystemUtil : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // var simulationSystemGroup = World.Active.GetExistingSystem<SimulationSystemGroup>();
        // var mainLoopSys = World.Active.CreateSystem<MainLoopSystem>();
        // simulationSystemGroup.AddSystemToUpdateList(mainLoopSys);
        // simulationSystemGroup.SortSystemUpdateList();
    }

    private void Start()
    {
        // World.Active.GetOrCreateSystem<MainLoopSystem>().Enabled = true;
        //
        // var _archetypeA = World.Active.EntityManager.CreateArchetype(typeof(Translation),typeof(Rotation),typeof(LocalToWorld));
        //
        //
        // World.Active.EntityManager.CreateEntity(_archetypeA);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
