﻿using UnityEngine;
using System.Collections;

public class Neurons : MonoBehaviour {


	private Brain brain;
	private NodeAI nAI;

	private bool checkDone = false;

	   // Current states for the AI
	private bool Idle;
	private bool Scanning;
	private bool Wandering;
	private bool Walking;
	private bool pickingUpBomb;
	private bool runningAway;
	private bool Hiding;
	private bool Panic;


	    // Components of AI memory
	private ArrayList hidingObjectMemory; // Seen hiding Objects
	private ArrayList occupiedMem;       // hididng objects that someone else is hiding in
	private ArrayList bombMemory;   // Bombs AI has seen 
	private GameObject visiblePlayer;  // Holds the player if visible
	private Vector2 lastKnownPlayerPos; // The last place the player was seen


	private GameObject targetBomb; // A bomb that AI plans to pick up


	private int wait; // Causing AI to wait a certain number of frames
	private int count;



	// Use this for initialization
	void Start () {
	
		GameObject nod = GameObject.Find ("Nodes");
		nAI = nod.GetComponent ("NodeAI") as NodeAI;
		brain = gameObject.GetComponent ("Brain") as Brain;
		hidingObjectMemory = new ArrayList ();
		occupiedMem = new ArrayList ();
		bombMemory = new ArrayList ();



		Idle = true;
		Scanning = false;
		Wandering = false;
		pickingUpBomb = false;
		runningAway = false;
		hiding = false;
		Panic = false;

		wait = 0;
		count = 0;

	}
	
	// Update is called once per frame
	void Update () 
	{
	
		ArrayList tempHiding = brain.hidingObjects ();
		ArrayList tempBombs = brain.getVisisbleBombs ();
		visiblePlayer = brain.visiblePlayer ();

		if(visiblePlayer != null) { lastKnownPlayerPos = visiblePlayer.transform.position; interruptAndRun(); }

		remember (hidingObjectMemory, tempHiding);
		remember (bombMemory, tempBombs);


		if(nAI.nodesDone && brain.IsRunning)
		{
			goodOlFashionAI();
		}
	}



	// UP = 0, DOWN = 1, LEFT = 2, RIGHT = 3
	private void check(int dir)
	{
		GameObject n = brain.closestNode ();
		NodeInfo nI;
		if(n!=null)
		{
			nI = n.GetComponent("NodeInfo") as NodeInfo;
			switch (dir)
			{
			  case 0: if(nI.up != null) brain.seek (n, nI.up); break;
			  case 1: if(nI.down != null) brain.seek (n, nI.down); break;
			  case 2: if(nI.left != null) brain.seek (n, nI.left); break;
			  case 3: if(nI.right != null) brain.seek (n, nI.right); break;
			}

		}
	}




	private void remember(ArrayList mem, ArrayList input)
	{
		if(input.Count > 0)
		{
			for(int i=0; i<input.Count; i++)
			{
				GameObject t = (GameObject)input[i];
				bombFunctions func = t.GetComponent("bombFunctions") as bombFunctions;
				bool markedBomb = false;

				if(func != null) markedBomb = func.isMarked();

				if (!mem.Contains(t) && !markedBomb && !occupiedMem.Contains(t)) mem.Add(t);
				else if(mem.Contains (t) && markedBomb) mem.Remove(t);
			}
		}
	}



	private void interruptAndRun()
	{
		wait = 0;
		if(Idle) Idle=false;
		if(Scanning) Scanning=false;
		if(Wandering) Wandering=false;
		if(Walking) { brain.interruptPath(); Walking=false; }
		if(pickingUpBomb)
		{
			if(targetBomb!=null)
			{
				bombFunctions f = targetBomb.GetComponent("bombFunctions") as bombFunctions;
				f.unMark();
				bombMemory.Add (targetBomb);
				targetBomb = null;
			}

			brain.interruptPath();
			pickingUpBomb = false;
		}

		runningAway = true; 
		brain.sprint ();
	}



	private void goodOlFashionAI()
	{
		if(wait == 0)
		{
			if (Idle) 
			{
				Scanning = true;
				Idle = false;
				count = 0;
			}
			
			if(Scanning)
			{
				if(count<4) { check(count); wait=20; count++;} 
				if(count == 4 && (bombMemory.Count==0 || brain.hasBomb())) { wait=0; Scanning=false; Wandering=true;}
				else if (count == 4 && bombMemory.Count>0 && !brain.hasBomb()) 
				{ 
					wait=0; 
					Scanning=false; pickingUpBomb=true; 

					GameObject closestBomb = null;
					for(int i=0; i<bombMemory.Count; i++)
					{
						GameObject t = (GameObject)bombMemory[i];

						if(closestBomb == null) closestBomb = t;

						else
						{

							if(Vector2.Distance (transform.position, t.transform.position) < Vector2.Distance(transform.position, closestBomb.transform.position))
								closestBomb = t;
						}
					}

					targetBomb = closestBomb;
					bombMemory.Remove (closestBomb);

					bombFunctions func = targetBomb.GetComponent("bombFunctions") as bombFunctions;
					func.mark();

					brain.seek (brain.closestNode(), func.closestNode());
				}
			}

			if(Wandering)
			{
				int chance = Random.Range(1, 101);
				if(chance>20 && chance<56) { Wandering=false; Idle=true; wait=15; }
				else
				{
					ArrayList n = brain.getVisibleNodes();
					if(n.Count == 0) { Wandering=false; Idle=true; }
					else if (n.Count > 0)
					{
						brain.seek (brain.closestNode(), (GameObject)n[Random.Range (0, n.Count)]);
						Wandering=false; Walking=true; wait=5;
					}
				}
			}


			if(Walking)
			{
				if (!brain.pathing) { Walking=false; Wandering=true; wait=5; }
			}


			if(pickingUpBomb)
			{
				if (!brain.pathing || targetBomb==null) 
				{
					if(targetBomb==null) { brain.interruptPath(); }
					else { brain.pickUpBomb(targetBomb); }

					pickingUpBomb=false; Idle=true; wait=5;
				}
			}


			if(runningAway)
			{
				if(hidingObjectMemory.Count>0)
				{
					GameObject furthest = null;

					for(int i=0; i<hidingObjectMemory.Count; i++)
					{
						GameObject t = (GameObject)hidingObjectMemory[i];

						if(furthest==null) furthest=t;

						else
						{
							if(Vector2.Distance (t.transform.position, lastKnownPlayerPos) > Vector2.Distance(furthest.transform.position, lastKnownPlayerPos))
								furthest=t;
						}
					}


				}
			}

		}


		if(wait > 0) wait--;

	}
	
}
