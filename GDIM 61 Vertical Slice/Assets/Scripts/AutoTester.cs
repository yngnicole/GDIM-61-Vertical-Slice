using System.Collections;
using UnityEngine;

/// <summary>
/// Press F1 to auto-test the full order flow.
/// Automatically created at runtime by GameBootstrap.
/// </summary>
public class AutoTester : MonoBehaviour
{
    bool _running = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !_running)
        {
            StartCoroutine(RunAutoTest());
        }
    }

    IEnumerator RunAutoTest()
    {
        _running = true;
        Debug.Log("=== AUTO TEST START ===");

        var mgr = OrderManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[AutoTest] FAIL: OrderManager.Instance is null");
            _running = false;
            yield break;
        }
        Debug.Log("[AutoTest] OrderManager found. State: " + mgr.CurrentState);

        // Wait for NPC to arrive and order bubble to appear
        Debug.Log("[AutoTest] Waiting for NPC to arrive...");
        OrderBubble bubble = null;
        float timeout = 10f;
        float elapsed = 0f;
        while (bubble == null && elapsed < timeout)
        {
            bubble = Object.FindObjectOfType<OrderBubble>();
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (bubble == null)
        {
            Debug.LogError("[AutoTest] FAIL: No OrderBubble found after " + timeout + "s");
            _running = false;
            yield break;
        }
        Debug.Log("[AutoTest] PASS: Order bubble appeared. NPC arrived.");
        yield return new WaitForSeconds(0.5f);

        // Step 1: Take order
        Debug.Log("[AutoTest] Step 1: Taking order...");
        mgr.TakeOrder(bubble.Owner);
        bubble.OnOrderTaken();
        if (mgr.CurrentState != OrderManager.PlayerState.OrderTaken)
        {
            Debug.LogError("[AutoTest] FAIL: State should be OrderTaken, is " + mgr.CurrentState);
            _running = false;
            yield break;
        }
        Debug.Log("[AutoTest] PASS: Order taken. State: " + mgr.CurrentState);
        yield return new WaitForSeconds(0.5f);

        // Step 2: Start brewing
        Debug.Log("[AutoTest] Step 2: Starting brew...");
        CoffeeMachine machine = Object.FindObjectOfType<CoffeeMachine>();
        if (machine == null)
        {
            Debug.LogError("[AutoTest] FAIL: No CoffeeMachine found in scene");
            _running = false;
            yield break;
        }
        mgr.StartBrewing(machine);
        if (mgr.CurrentState != OrderManager.PlayerState.Brewing)
        {
            Debug.LogError("[AutoTest] FAIL: State should be Brewing, is " + mgr.CurrentState);
            _running = false;
            yield break;
        }
        Debug.Log("[AutoTest] PASS: Brewing started on " + machine.gameObject.name);

        // Step 3: Wait for brew to complete
        Debug.Log("[AutoTest] Step 3: Waiting for brew...");
        timeout = 10f;
        elapsed = 0f;
        while (mgr.CurrentState == OrderManager.PlayerState.Brewing && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (mgr.CurrentState != OrderManager.PlayerState.DrinkReady)
        {
            Debug.LogError("[AutoTest] FAIL: State should be DrinkReady, is " + mgr.CurrentState);
            _running = false;
            yield break;
        }
        Debug.Log("[AutoTest] PASS: Brew complete! State: " + mgr.CurrentState);
        yield return new WaitForSeconds(0.5f);

        // Step 4: Pick up drink
        Debug.Log("[AutoTest] Step 4: Picking up drink...");
        mgr.PickUpDrink();
        machine.OnPickedUp();
        if (mgr.CurrentState != OrderManager.PlayerState.HoldingDrink)
        {
            Debug.LogError("[AutoTest] FAIL: State should be HoldingDrink, is " + mgr.CurrentState);
            _running = false;
            yield break;
        }
        Debug.Log("[AutoTest] PASS: Drink picked up. State: " + mgr.CurrentState);
        yield return new WaitForSeconds(0.5f);

        // Step 5: Deliver to NPC
        Debug.Log("[AutoTest] Step 5: Delivering order...");
        NPC activeNPC = mgr.ActiveNPC;
        if (activeNPC == null)
        {
            Debug.LogError("[AutoTest] FAIL: ActiveNPC is null");
            _running = false;
            yield break;
        }
        int moneyBefore = mgr.Money;
        mgr.FulfillOrder(activeNPC);
        if (mgr.CurrentState != OrderManager.PlayerState.Idle)
        {
            Debug.LogError("[AutoTest] FAIL: State should be Idle, is " + mgr.CurrentState);
            _running = false;
            yield break;
        }
        if (mgr.Money <= moneyBefore)
        {
            Debug.LogError("[AutoTest] FAIL: Money didn't increase. Before: " + moneyBefore + " After: " + mgr.Money);
            _running = false;
            yield break;
        }
        Debug.Log("[AutoTest] PASS: Order delivered! Money: $" + mgr.Money);

        Debug.Log("=== AUTO TEST COMPLETE: ALL PASSED ===");
        _running = false;
    }
}
