using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TheKiwiCoder {

    // The context is a shared object every node has access to.
    // Commonly used components and subsytems should be stored here
    // It will be somewhat specific to your game exactly what to add here.
    // Feel free to extend this class 
    public class Context {
        //TODO: remove the placeholder stuff that isn't used in our implementation.
        public GameObject gameObject;
        public Transform transform;
        public Animator animator;
        public Rigidbody physics;
        public SphereCollider sphereCollider;
        public BoxCollider boxCollider;
        public CapsuleCollider capsuleCollider;
        public CharacterController characterController;
        // Add other game specific systems here
        public SAi SAi;
        public APlayer aPlayer;

        public static Context CreateFromGameObject(GameObject gameObject) {
            //TODO: remove the placeholder stuff that isn't used in our implementation.
            // Fetch all commonly used components
            Context context = new Context();
            context.gameObject = gameObject;
            context.transform = gameObject.transform;
            context.animator = gameObject.GetComponent<Animator>();
            context.physics = gameObject.GetComponent<Rigidbody>();
            context.sphereCollider = gameObject.GetComponent<SphereCollider>();
            context.boxCollider = gameObject.GetComponent<BoxCollider>();
            context.capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
            context.characterController = gameObject.GetComponent<CharacterController>();
            
            // Add whatever else you need here...
            context.SAi = gameObject.GetComponent<SAi>();
            if (GameManager.Instance) context.aPlayer = GameManager.Instance.aPlayer;

            return context;
        }
    }
}