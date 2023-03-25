using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour,IKitchenObjectParent
{

    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }
    [SerializeField]private float moveSpeed = 7f;
    [SerializeField]private GameInput gameInput;
    [SerializeField]private LayerMask CountersLayer;
    [SerializeField] private Transform playerObjectHoldPoint;

    private KitchenObject kitchenObject;

    private bool isWalking;
    private BaseCounter selectedCounter;
    private Vector3 lastInteractDirection;


    private void Awake() {
        if(Instance != null){
            Debug.LogError("There is more than one player instance!");
        }
        Instance = this;
    }

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAciton;
    }

    private void GameInput_OnInteractAciton(object sender , System.EventArgs e){
        if(selectedCounter != null){
            selectedCounter.Interact(this);
        }
         
    }
    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking(){
        return isWalking;
    }

    private void HandleInteractions(){
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector.x,0f,inputVector.y);

        if(moveDirection != Vector3.zero){
            lastInteractDirection = moveDirection;
        }

        float interactDistance = 2f;
        if(Physics.Raycast(transform.position , lastInteractDirection , out RaycastHit raycastHit , interactDistance , CountersLayer)){
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)){
                if(selectedCounter != baseCounter){
                    SetSelectedCounter(baseCounter);
                }
            }else{
                SetSelectedCounter(null);
            }
        }else{
            SetSelectedCounter(null);
        }
        
    }
    private void HandleMovement(){
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector.x,0f,inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerHeight = 1.8f;
        float playerRadius = .7f;
        bool canMove = !Physics.CapsuleCast(transform.position , transform.position + Vector3.up * playerHeight , playerRadius , moveDirection , moveDistance);
        
        if(!canMove){
            //can't move
            //Attempt moving in only X axis
            Vector3 moveDirectionX = new Vector3(moveDirection.x,0,0);
            canMove = !Physics.CapsuleCast(transform.position , transform.position + Vector3.up * playerHeight , playerRadius , moveDirectionX , moveDistance);
            if(canMove){
                moveDirection = moveDirectionX;
            }
            else{
                //Can't move in the X axis
                //Attempt moving in the only Z axis
                Vector3 moveDirectionZ = new Vector3(0,0,moveDirection.z);
                canMove = !Physics.CapsuleCast(transform.position , transform.position + Vector3.up * playerHeight , playerRadius , moveDirectionZ , moveDistance);
                if(canMove){
                    moveDirection = moveDirectionZ;
                }else{
                    //Can't move in any Axis
                }

            }
        }
        if(canMove){
        transform.position += moveDirection * moveDistance;
        }

        isWalking = moveDirection != Vector3.zero;
        
        float rotateSpeed = 10f;
        if(isWalking)
        transform.forward = Vector3.Slerp(transform.forward,moveDirection,Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter){
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform(){
        return playerObjectHoldPoint;
    }
    
    public KitchenObject GetKitchenObject(){
        return kitchenObject;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        this.kitchenObject = kitchenObject;
    }

    public void ClearKitchenObject(){
        this.kitchenObject = null;
    }

    public bool HasKitchenObject(){
        return kitchenObject != null;
    }
}
