using System;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private struct V3Objective { public bool Active,Egg; public int Id,Source,Target; public Vector2 Position; public float Until,Radius; }
        private readonly V3Objective[] v3Objectives=new V3Objective[RulesV3Tuning.ConstructCapacity];
        private void StartRulesV3Objectives()
        {
            Array.Clear(v3Objectives,0,v3Objectives.Length);
            EnemyRulesV3.DangerRequested+=warning=>
            {if(warning.Rule=="egg")AddV3Objective(warning.RequestId,warning.SourceId,0,warning.Position,warning.WarningTime,warning.Radius,true);};
            SkillsV3.CastRequested+=cast=>V3StrikeObjectives(cast.Position,cast.End,cast.Radius,cast.Shape);
        }
        private void AddV3Objective(int id,int source,int target,Vector2 position,float duration,float radius,bool egg)
        {
            for(int i=0;i<v3Objectives.Length;i++)if(!v3Objectives[i].Active || v3Objectives[i].Until<=Elapsed)
            {v3Objectives[i]=new V3Objective {Active=true,Id=id,Source=source,Target=target,Position=position,Until=Elapsed+duration,Radius=radius,Egg=egg};return;}
        }
        public bool StrikeRulesV3Objective(int id)
        {
            for(int i=0;i<v3Objectives.Length;i++)if(v3Objectives[i].Active && v3Objectives[i].Id==id)
            {
                V3Objective objective=v3Objectives[i];v3Objectives[i].Active=false;
                return objective.Egg?EnemyRulesV3.CancelEgg(id):EnemyRulesV3.CutChain(objective.Source);
            }
            return false;
        }
        private void V3StrikeObjectives(Vector2 start,Vector2 end,float radius,SkillShape shape)
        {
            for(int i=0;i<v3Objectives.Length;i++)
            {
                V3Objective objective=v3Objectives[i];if(!objective.Active || objective.Until<=Elapsed)continue;
                Vector2 point=objective.Position;
                if(!objective.Egg)
                {UnitState a=UnitWithId(objective.Source),b=UnitWithId(objective.Target);if(a==null || b==null)continue;point=(a.Position+b.Position)*.5f;}
                if(CompanionSkillRuntime.Contains(shape,start,end,radius+objective.Radius,point))StrikeRulesV3Objective(objective.Id);
            }
        }
        private static float V3Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        private static bool V3SegmentsCross(Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            Vector2 ab=b-a,cd=d-c;float determinant=V3Cross(ab,cd);
            if(Mathf.Abs(determinant)<=RulesV3Tuning.Epsilon)return false;
            float t=V3Cross(c-a,cd)/determinant,u=V3Cross(c-a,ab)/determinant;
            return t>=0 && t<=1 && u>=0 && u<=1;
        }
        private bool V3WallBlocksMove(UnitState enemy,Vector2 previous)
        {
            if(EnemyRulesV3?.IgnoresWallsAndTaunt(enemy)??false)return false;
            for(int i=0;i<v3Constructs.Length;i++)
            {V3Construct wall=v3Constructs[i];if(wall.Active && wall.Bodies && wall.Until>Elapsed && V3SegmentsCross(previous,enemy.Position,wall.Start,wall.End))return true;}
            return false;
        }
    }
}
