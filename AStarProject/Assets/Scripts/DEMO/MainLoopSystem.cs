using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

//[ExecuteAlways]
//[AlwaysUpdateSystem]
[DisableAutoCreation]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class MainLoopSystem : ComponentSystem
{
   private BeginSimulationEntityCommandBufferSystem _commandBufferSystem;
   private EntityQuery _query;
   public EntityArchetype _archetypeA;

   private int EntityCountMax = 10;
   
   protected override void OnCreate()
   {
      base.OnCreate();
      _commandBufferSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
      _query = GetEntityQuery(typeof(Translation),typeof(Rotation));
      _archetypeA = World.Active.EntityManager.CreateArchetype(typeof(Translation),typeof(Rotation),typeof(LocalToWorld));
   }

   protected override void OnStartRunning()
   {
      base.OnStartRunning();
   }

   protected override void OnUpdate()
   {
      // while (EntityCountMax > 0)
      // {
      //    Entity a = P       ostUpdateCommands.CreateEntity(_archetypeA);
      //    PostUpdateCommands.SetComponent(a,new Translation()
      //    {
      //       Value = new float3(EntityCountMax * 1,EntityCountMax * 1,EntityCountMax * 1)
      //    });
      //
      //
      //    EntityCountMax--;
      // }
      //
      // int curFrameECount = 0;
      // Entities.With(_query).ForEach((Entity entity, ref Translation translation) =>
      // {
      //    curFrameECount++;
      // });
      //
      // Debug.Log(string.Format(" FrameCount {0} ,Entitys count {1}",Time.frameCount,curFrameECount));
   }

   protected override void OnStopRunning()
   {
      base.OnStopRunning();
   }

   protected override void OnDestroy()
   {
      base.OnDestroy();
   }
}
