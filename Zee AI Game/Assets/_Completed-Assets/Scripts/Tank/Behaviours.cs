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
                    return MrSuicide(); // Fun();
                case 1:
                    return PossiblyDeadly(); //Deadly();
                case 2:
                    return TooFrightened(); //Frightened();
                case 3:
                    return TrulyFunny(tankSpeed); //Unpredictable();


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
        private Root MrSuicide()
        {
            //My initial idea was to create a suicidal AI for this and that is what I achieved.
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                            new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER_OR_EQUAL, 0.5f, Stops.IMMEDIATE_RESTART,
                            //if the AI tank's centre position is facing the non AI within 0.5f (pixels) then stop turning, wait 
                            //and then randomly fire
                            new Sequence(StopTurning(), new Wait(1f), RandomFire())),

                        new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, new Action(() => Turn(0.9f))),
                        //if non AI tank is to the right of the AI controlled tank then turn to the right

                        new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, new Action(() => Turn(-0.9f)))
                        //if the non AI tank is not to the right of the AI controlled tank then turn left.                      
                          
                    )
                )
            );
        }
//---------------------------------------------------------------------------------------------------------------------------------------------

//---------------------------------------------------------------Deadly behaviour ----------------------------------------------------------------
        private Root PossiblyDeadly()
        {
            return new Root(
                new Service(0.2f, UpdatePerception, new Selector(
                    
                             new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER_OR_EQUAL, 0.1f, Stops.IMMEDIATE_RESTART,
                             //if non AI controlled tank is facing the AI controlled tank within 0.1f then a sequence begins with stop turning, wait 
                             //and then randomly shoot. 
                             
                             new Sequence(StopTurning(), new Wait(1f), RandomFire())
                             ),

                             new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                             //if the tank is to the right of the enemy tank then turn right
                             new Action(() => Turn(0.2f))),

                             new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                             //if the tank is NOT to the right of the enemy tank then turn left
                             new Action(() => Turn(-0.2f))),

                            new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 15f, Stops.IMMEDIATE_RESTART, 
                            //if the enemy is NOT within the distance field of 15f (pixels) then move towards the enemy
                            new Action(() => Move(0.1f))),

                            new BlackboardCondition("targetInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                           // LOOK FOR ENEMY, if enemy is in front then stop turning and shoot
                           // new Sequence(StopTurning(), new Wait(2f), RandomFire()) (was going to use this)
                           new Sequence(StopTurning()) 
                            )
                )
                )
            );
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------

        //---------------------------------------------------------------Frightened behaviour ----------------------------------------------------------------
        private Root TooFrightened()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(

                        new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 25f, Stops.IMMEDIATE_RESTART,
                             new Action(() => Move(-1f))),
                        //if the non AI controlled tank is within the distance field of 25f then move away and back off.
                    
   
                        new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 50f, Stops.IMMEDIATE_RESTART,
                             new Sequence(StopTurning(), new Wait(0.5f), RandomFire()))
                             //if the non AI controlled tank is even more within the distance field of 50 this time then stop turning, wait 
                             //and randomly fire

                        
                           //it will just act scared, shoot and end up being cornered
                        
                    )
                )
            );
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------

        //---------------------------------------------------------------Unpredictable behaviour ----------------------------------------------------------------
        private Root TrulyFunny(float randomMove)
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(

                        //Wasn't too sure what to do for unpredictable without using too many random floats so I decided to go with this and simply
                        // go with this outcome and make it difficult to kill. 

                        new BlackboardCondition("targetInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                        //if the target is in front then turn towards, move and shoot.

                            new Sequence(new Action(() => Turn(randomMove)), new Action(() => Move(1)), new Action(() => Fire(1)))),



                          new BlackboardCondition("targetInFront", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                          //if the rarget isn't in front then turn, move and shoot. 

                            new Sequence(new Action(() => Turn(randomMove)), new Action(() => Move(-1)), new Action(() => Fire(1)))),




                            new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,

                            new Sequence(new Action(() => Turn(0.2f)), new Action(() => Move(-1)), new Action(() => Fire(1)))),


                            new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,

                            new Sequence(new Action(() => Turn(0.2f)), new Action(() => Move(-1)), new Action(() => Fire(1))))
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