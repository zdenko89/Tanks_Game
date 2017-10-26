using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
    /*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
    public partial class TankAI : MonoBehaviour
    {
        private Root CreateBehaviourTree() {

            System.Random randomMove = new System.Random();
            float tankSpeed = randomMove.Next(-1, 1);

            switch (m_Behaviour) {

                case 4:
                    return SpinBehaviour(-0.05f, 1f);
                case 5:
                    return TrackBehaviour();
                case 0:
                    return Fun();
                case 1:
                    return Deadly(); 
                case 2:
                    return Frightened();
                case 3:
                    return Unpredictable(tankSpeed);
                

                default:
                    return new Root (new Action(()=> Turn(0.1f)));
            }
        }

        /* Actions */

        private Node StopTurning() {
            return new Action(() => Turn(0));
        }

        private Node RandomFire() {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }


        /* Example behaviour trees */

        // Constantly spin and fire on the spot 
        private Root SpinBehaviour(float turn, float shoot) {
            return new Root(new Sequence(
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                    ));
        }

        // Turn to face your opponent and fire
        private Root TrackBehaviour() {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER_OR_EQUAL, 0.1f, Stops.IMMEDIATE_RESTART,
                            // Stop turning and fire
                            new Sequence(StopTurning(), new Wait(2f), RandomFire())),
                            new BlackboardCondition("targetOnRight",  Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,  new Action(() => Turn(0.2f))),
                            // Turn left toward target
                            new Action(() => Turn(-0.2f))
                    )
                )
            );
        }

//---------------------------------------------------------------Fun behaviour ---------------------------------------------------------------
        private Root Fun()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                            new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER_OR_EQUAL, 0.5f, Stops.IMMEDIATE_RESTART,

                            new Sequence(StopTurning(), new Wait(1f), RandomFire())),

                        new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, new Action(() => Turn(0.9f))),

                        new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, new Action(() => Turn(0.9f)))
                                             
                          
                    )
                )
            );
        }
//---------------------------------------------------------------------------------------------------------------------------------------------

//---------------------------------------------------------------Deadly behaviour ----------------------------------------------------------------
        private Root Deadly()
        {
            return new Root(
                new Service(0.2f, UpdatePerception, new Selector(
                    
                             new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER_OR_EQUAL, 0.1f, Stops.IMMEDIATE_RESTART,
                             //if tank is not facing the enemy then turn
                             //new Action(() => Turn(0.2f))
                             new Sequence(StopTurning(), new Wait(1f), RandomFire())
                             ),

                             new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                             //if tank is not facing the enemy then turn
                             new Action(() => Turn(0.2f))),

                             new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                             //if tank is not facing the enemy then turn
                             new Action(() => Turn(-0.2f))),

                            new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 15f, Stops.IMMEDIATE_RESTART, 
                            //if the enemy is NOT the distance field then move towards the enemy
                            new Action(() => Move(0.1f))),

                            new BlackboardCondition("targetInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                           // LOOK FOR ENEMY, if enemy is in front then stop turning and shoot
                           // new Sequence(StopTurning(), new Wait(2f), RandomFire())
                           new Sequence(StopTurning())
                            )
                )
                )
            );
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------

        //---------------------------------------------------------------Frightened behaviour ----------------------------------------------------------------
        private Root Frightened()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(

                        new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 25f, Stops.IMMEDIATE_RESTART,
                             new Action(() => Move(-1f))),

                        new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 50f, Stops.IMMEDIATE_RESTART,
                             new Sequence(StopTurning(), new Wait(0.5f), RandomFire()))

                        
                           //it will just act scared, shoot and end up being cornered
                        
                    )
                )
            );
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------

        //---------------------------------------------------------------Unpredictable behaviour ----------------------------------------------------------------
        private Root Unpredictable(float randomMove)
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(

                        new BlackboardCondition("targetInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,

                            new Sequence(new Action(() => Move(1)), new Action(() => Fire(1))))

                            

                                                        
                    )
                )
            );
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdatePerception() {
            Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;
            blackboard["targetDistance"] = localPos.magnitude;
            blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;
            blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
        }

    }
}