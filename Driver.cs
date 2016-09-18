using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine.UI;

//The 14 different OutputNeurons with named purpose
public enum OutputNeurons {
    A_ROCK,
    A_PAPER,
    A_SCISSORS,
    D_ROCK,
    D_PAPER,
    D_SCISSORS,
    UP_LEFT,
    UP_CENTER,
    UP_RIGHT,
    MID_LEFT,
    MID_RIGHT,
    DOWN_LEFT,
    DOWN_CENTER,
    DOWN_RIGHT
}

//Used to determine what defense beats what attack
public static class GameHandler {
    public const int NUM_ATTACKS = 3;

    //Returns what defense ties the given attack
    public static OutputNeurons tiesTo(OutputNeurons attack) {
        return attack + NUM_ATTACKS;
    }

    //Returns what defense beats the given attack
    public static OutputNeurons beats(OutputNeurons attack) {
        return (OutputNeurons)Utils.mod((int)attack + 1, NUM_ATTACKS) + NUM_ATTACKS;
    }

    //Returns what defense loses to the given attack
    public static OutputNeurons losesTo(OutputNeurons attack) {
        return (OutputNeurons)Utils.mod((int)attack + 2, NUM_ATTACKS) + NUM_ATTACKS;
    }
}

//Drives the game
public class Driver : MonoBehaviour {
	public static Driver S; //Singleton

	public bool startNew = false;

    public int startingPlayers;

	public int startingInnovation;

	public int newSpeciesTime;

	public int turnTime;

	public const float CELL_SIZE = .1f;

	public const int GRID_SIZE = 100;

	public GameObject cellPrefab;

    public GameObject UIParent;

    [SerializeField]
	public List<PreGenome> genomes = new List<PreGenome> ();

	[SerializeField]
	public List<string> names = new List<string> ();

	public List<string> useNames = new List<string> ();

	public bool _________________________________;

	private Dictionary<string, Genome> genomeMap = new Dictionary<string, Genome>();
    private Dictionary<string, int> turnsUntilMove = new Dictionary<string, int>();
	private List<Genome> sortedList = new List<Genome> ();

	public List<List<Cell>> cells;

	private bool isPaused = false;

    private System.IO.StreamWriter deadFile;

    private System.IO.StreamWriter endFile;

	// Use this for initialization
	void Awake () {
		Application.runInBackground = true;

        File.Delete(Application.dataPath + "\\deadPlayers.txt");
        deadFile = new StreamWriter(Application.dataPath + "\\deadPlayers.txt", true);

        File.Delete(Application.dataPath + "\\alivePlayers.txt");
        endFile = new StreamWriter(Application.dataPath + "\\alivePlayers.txt", true);

        S = this;

		InvasionLearning.currentInnovation = startingInnovation;

		useNames = new List<string> (names);

        InitializeGrid();

		initializeGame ();

		Invoke ("playTurn", turnTime);
		InvokeRepeating ("addPlayer", newSpeciesTime, newSpeciesTime);
	}

    void Update() {
        if(Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    void OnApplicationQuit() {
        saveCurrentPlayers();
    }

    //Sets up the grid of cells
    private void InitializeGrid() {
        cells = new List<List<Cell>>();
        cells.Capacity = GRID_SIZE;

        //Initialize each row
        for (int i = 0; i < cells.Capacity; ++i) {
            cells.Add(new List<Cell>());
            cells[i].Capacity = GRID_SIZE;
        }

        //Add each cell to each location and to the game world
        for (int i = 0; i < GRID_SIZE; ++i) {
            for (int j = 0; j < GRID_SIZE; ++j) {
                GameObject cellObject = Instantiate(cellPrefab, new Vector3(j * CELL_SIZE, i * CELL_SIZE, 0), Quaternion.identity) as GameObject;

                cells[i].Add(cellObject.GetComponent<Cell>());
            }
        }
    }

    //Runs one turn for the entire board
	public void playTurn() {
		float startTurnTime = Time.fixedTime;

		if(isPaused) return;

        //Play turn for each cell in the board
		for (int i = 0; i < GRID_SIZE; ++i) {
			for (int j = 0; j < GRID_SIZE; ++j) {
                if (genomeMap.ContainsKey(cells[i][j].playerName)) {
                    if (turnsUntilMove[cells[i][j].playerName] == 0) {
                        playCellTurn(new Vector2(i, j));
                    }
                }
			}
		}

        //Determine fitness for each player on the board
        clearFitness();
        countFitness();

        //Update the turns until next move for players that made a play this turn
        updateTurnsUntilMove();

        clearUnfit();

        updateScoreboard();

        resetBoard();

        //print (Time.fixedTime - startTurnTime);
        Invoke ("playTurn",  Mathf.Max (Time.fixedTime - startTurnTime, turnTime));
	}

    //Plays a turn for the cell at location
    public void playCellTurn(Vector2 location) {
        Cell current = cells[(int) location.x][(int) location.y];

        if (!current.active) return;

        string name = current.playerName;
        if (genomeMap.ContainsKey(name)) {
            //Current cell is owned by a player currently playing
            //Evaluate the cell and return it's decisions
            List<int> decisions = EvaluateCell(location);

            //If the player for some reason makes no decision
            //This should be an error
            if (decisions == null) return;

            Vector2 targetLocation = getCellTarget(location, decisions);
            runCellAttack(current, decisions, targetLocation);
        }
    }

    //Determine where the cell wants to attack
    public Vector2 getCellTarget(Vector2 location, List<int> decisions) {
        Vector2 target = new Vector2();
        int i = (int)location.x;
        int j = (int)location.y;
        switch (decisions[2]) {
            case ((int)OutputNeurons.UP_LEFT):
                target.x = Utils.mod(i - 1, GRID_SIZE);
                target.y = Utils.mod(j - 1, GRID_SIZE);
                break;
            case ((int)OutputNeurons.UP_CENTER):
                target.x = Utils.mod(i, GRID_SIZE);
                target.y = Utils.mod(j - 1, GRID_SIZE);
                break;
            case ((int)OutputNeurons.UP_RIGHT):
                target.x = Utils.mod(i + 1, GRID_SIZE);
                target.y = Utils.mod(j - 1, GRID_SIZE);
                break;
            case ((int)OutputNeurons.MID_LEFT):
                target.x = Utils.mod(i - 1, GRID_SIZE);
                target.y = Utils.mod(j, GRID_SIZE);
                break;
            case ((int)OutputNeurons.MID_RIGHT):
                target.x = Utils.mod(i + 1, GRID_SIZE);
                target.y = Utils.mod(j, GRID_SIZE);
                break;
            case ((int)OutputNeurons.DOWN_LEFT):
                target.x = Utils.mod(i - 1, GRID_SIZE);
                target.y = Utils.mod(j + 1, GRID_SIZE);
                break;
            case ((int)OutputNeurons.DOWN_CENTER):
                target.x = Utils.mod(i, GRID_SIZE);
                target.y = Utils.mod(j + 1, GRID_SIZE);
                break;
            case ((int)OutputNeurons.DOWN_RIGHT):
                target.x = Utils.mod(i + 1, GRID_SIZE);
                target.y = Utils.mod(j + 1, GRID_SIZE);
                break;
        }

        return target;
    }

    //Perform the attack and any subsequent actions
    public void runCellAttack(Cell current, List<int> decisions, Vector2 targetLocation) {
        int xLoc = (int)targetLocation.x;
        int yLoc = (int)targetLocation.y;

        //If attacking another cell, do not expand, instead handle attack
        if (genomeMap.ContainsKey(cells[xLoc][yLoc].playerName)) {
            //Determine what the other cell defends with
            List<int> otherCell = EvaluateCell(targetLocation);

            if (otherCell[1] == (int)GameHandler.beats((OutputNeurons)decisions[0])) {
                //Loss
                //This cell is destroyed in the failed attack
                current.color = Color.black;
                current.playerName = "";
            } else if (otherCell[1] == (int)GameHandler.tiesTo((OutputNeurons)decisions[0])) {
                //Tie
                //Nothing happens on a tie
            } else if (otherCell[1] == (int)GameHandler.losesTo((OutputNeurons)decisions[0])) {
                //Win
                //Cell being attacked is destroyed
                //First let the cell being attacked run its move
                //This ensures that there are no infinite loops while doing this
                current.active = false;
                //Everything will be set to enabled again at the end of the board's turn
                if(cells[xLoc][yLoc].active) {
                    playCellTurn(targetLocation);
                }
                //Now destroy the cell we attacked
                cells[xLoc][yLoc].playerName = "";
                cells[xLoc][yLoc].color = Color.black;
            }
        } else {
            //Expand if an empty square was attack
            cells[xLoc][yLoc].playerName = current.playerName;
            cells[xLoc][yLoc].color = new Color(genomeMap[current.playerName].r, genomeMap[current.playerName].g, genomeMap[current.playerName].b);
            cells[xLoc][yLoc].active = false;
        }
    }

    //Determine the three decisions of the cell at x, y
	public List<int> EvaluateCell(Vector2 location) {
        int x = (int)location.x;
        int y = (int)location.y;

		Cell current = cells [x] [y];

        //If this location is not owned by a current player, return nothing
		if(!genomeMap.ContainsKey(current.playerName)) return null;

		List<double> inputs = DetermineInputs (location);

		List<double> outputs = genomeMap [current.playerName].network.evaluateNetwork (inputs);

        //Find the list of the highest weight(s) for each decision
        List<int> attackMax = highestWeights(outputs, (int)OutputNeurons.A_ROCK, (int)OutputNeurons.A_SCISSORS);
        List<int> defendMax = highestWeights(outputs, (int)OutputNeurons.D_ROCK, (int)OutputNeurons.D_SCISSORS);
        List<int> locationMax = highestWeights(outputs, (int)OutputNeurons.UP_LEFT, (int)OutputNeurons.DOWN_RIGHT);

        //Randomly chose from the heighest weight(s) to decide on the three different decisions
        List<int> decisions = new List<int>();
		decisions.Add (attackMax[(int)(TestSimpleRNG.SimpleRNG.GetUniform() * (attackMax.Count))]);
        decisions.Add(defendMax[(int)(TestSimpleRNG.SimpleRNG.GetUniform() * (defendMax.Count))]);
        decisions.Add (locationMax[(int)(TestSimpleRNG.SimpleRNG.GetUniform() * (locationMax.Count))]);

		return decisions;
	}

    //Return a list of the highest weight(s) for the given list from [start,end]
    public List<int> highestWeights(List<double> weights, int start, int end) {
        List<int> weightMax = new List<int>();
        weightMax.Add(start);
        for (int i = start; i <= end; ++i) {
            if (weights[i] > weights[weightMax[0]]) {
                weightMax.Clear();
                weightMax.Add(i);
            } else if (weights[i] == weights[weightMax[0]]) {
                weightMax.Add(i);
            }
        }

        return weightMax;
    }

    //Create a list of inputs from the location given
	public List<double> DetermineInputs(Vector2 location) {
        int i = (int)location.x;
        int j = (int)location.y;

		List<double> values = new List<double> ();
		
		for (int x = Utils.mod (i-1, GRID_SIZE); x <= Utils.mod (i+1, GRID_SIZE); ++x) {
			for (int y = Utils.mod (j-1, GRID_SIZE); y <= Utils.mod (j+1, GRID_SIZE); ++y) {
				if (x != i || y != j) {
					if (cells [x] [y].playerName == "") {
						values.Add (0.0);
					} else if (cells [x] [y].playerName == cells[i][j].playerName) {
						values.Add (-1.0);
					} else if (cells [x] [y].playerName != cells[i][j].playerName) {
						values.Add (1.0);
					}
				}
			}
		}
		
		values.Add (1.0);

		return values;
	}

    //Compare function for two genomes based on their fitness
	public int CompareFitness(Genome g1, Genome g2) {
		if (g1.fitness < g2.fitness) {
			return 1;
		} else if (g1.fitness > g2.fitness) {
			return -1;
		} else {
			return 0;
		}
	}

    //Add either a fresh player or a child of two existing players
	public void addPlayer() {
		System.Random r = new System.Random ();

		if (r.Next (2) < 1) {
			print ("Driver.288 fresh player added");
			addPlayer (new Genome ("ancestor1", "", InvasionLearning.GENOME_SETTINGS, InvasionLearning.NEURAL_SETTINGS),
                       new Genome ("ancestor2", "", InvasionLearning.GENOME_SETTINGS, InvasionLearning.NEURAL_SETTINGS));
			return;
		}

        //Look within the top quarter of players sorted by fitness
		Genome p1 = sortedList[r.Next (sortedList.Count / 4)];
		Genome p2 = sortedList[r.Next (sortedList.Count / 4)];

		print ("Driver.293 child added " + p1.name + " " + p2.name);

		addPlayer (p1, p2);
	}

    //Add a specific genome to the game
	private void addPlayer(Genome newPlayer) {
		
		genomeMap.Add (newPlayer.name, newPlayer);
        turnsUntilMove.Add(newPlayer.name, 0);
		sortedList.Add (newPlayer);
		
		int x = (int)(UnityEngine.Random.value * cells [0].Count);
		int y = (int)(UnityEngine.Random.value * cells.Count);
		
		if(x == cells[0].Count) {
			x = cells[0].Count - 1;
		}
		
		if(y == cells[0].Count) {
			y = cells.Count - 1;
		}
		
        //Add 9 tiles centered around the x and y found
		for (int i = Utils.mod (x - 1, GRID_SIZE); i <= Utils.mod (x + 1, GRID_SIZE); ++i) {
			for(int j = Utils.mod (y - 1, GRID_SIZE); j <= Utils.mod (y + 1, GRID_SIZE); ++j) {
				cells[i][j].playerName = newPlayer.name;
				cells[i][j].color = new Color(newPlayer.r, newPlayer.g, newPlayer.b);
			}
		}
	}

    //Add a child of two specific genomes to the game
	private void addPlayer(Genome p1, Genome p2) {
		Genome parentOne = p1;
		Genome parentTwo = p2;

		System.Random r = new System.Random ();

		Genome newPlayer = new Genome (useNames [r.Next(useNames.Count)], parentOne, parentTwo, 
                                       InvasionLearning.GENOME_SETTINGS, InvasionLearning.NEURAL_SETTINGS);

		useNames.Remove (newPlayer.name);

		newPlayer.r = UnityEngine.Random.value;
		newPlayer.g = UnityEngine.Random.value;
		newPlayer.b = UnityEngine.Random.value;

		addPlayer (newPlayer);
	}

    //Add all preset players or create new player if the game is to start new
	public void initializeGame() {
		if (!startNew) {
			foreach (PreGenome pG in genomes) {
				addPlayer (pG.generateGenome (InvasionLearning.GENOME_SETTINGS, InvasionLearning.NEURAL_SETTINGS));
			}
		}

		if (startNew) {
			for (int i = 0; i < startingPlayers; ++i) {
				addPlayer (new Genome ("ancestor1", "", InvasionLearning.GENOME_SETTINGS, InvasionLearning.NEURAL_SETTINGS),
                           new Genome ("ancestor2", "", InvasionLearning.GENOME_SETTINGS, InvasionLearning.NEURAL_SETTINGS));
			}
		}
	}

    //Remove all fitness from every player
    public void clearFitness() {
        foreach (string s in genomeMap.Keys) {
            genomeMap[s].fitness = 0;
        }
    }

    //Count the fitness for each player updating their Genome
    public void countFitness() {
        for (int i = 0; i < GRID_SIZE; ++i) {
            for (int j = 0; j < GRID_SIZE; ++j) {
                cells[i][j].active = true;
                if (genomeMap.ContainsKey(cells[i][j].playerName)) {
                    genomeMap[cells[i][j].playerName].fitness++;
                }
            }
        }
    }

    //Clear any player with 0 fitness or in the bottom 15 players with worse fitness than initial
    public void clearUnfit() {
        List<KeyValuePair<string, Genome>> list = genomeMap.ToList();
        foreach (KeyValuePair<string, Genome> pair in list) {
            if (pair.Value.fitness == 0 || (list.IndexOf(pair) < list.Count - 15 && pair.Value.fitness < 9)) {
                removePlayer(pair.Key);
            }
        }
    }

    //Rearrange the scoreboard for all players based on fitness
    public void updateScoreboard() {
        if (UIParent == null) {
            print("ERROR: CanvasUI not found");
        }

        sortedList.Sort(CompareFitness);

        for (int i = 0; i < 20 /*&& i < sortedList.Count*/; ++i) {
            GameObject rank = UIParent.transform.Find("RankUI").transform.Find("Rank" + (i + 1)).gameObject;

            if (i >= sortedList.Count) {
                rank.GetComponent<Text>().text = (i + 1) + ".";
                rank.transform.Find("Panel").gameObject.GetComponent<Image>().color = Color.black;
            } else {
                rank.GetComponent<Text>().text = (i + 1) + "." + sortedList[i].name;
                rank.transform.Find("Panel").gameObject.GetComponent<Image>().color = new Color(sortedList[i].r,
                                                                                                sortedList[i].g,
                                                                                                sortedList[i].b);
            }
        }
    }
    
    //Updates all player's turns until next move
    public void updateTurnsUntilMove() {
        foreach(string name in genomeMap.Keys) {
            if(turnsUntilMove[name] == 0) {
                turnsUntilMove[name] = updateTurnsUntilMove(name);
            } else {
                --turnsUntilMove[name];
            }
        }
    }

    public int updateTurnsUntilMove(string key) {
        return (int) Math.Pow(InvasionLearning.BLUE_SHELL, genomeMap[key].fitness) - 1;
    }

    //Remove player with name _name from the game board and list
	public void removePlayer(string _name) {
		if (!genomeMap.ContainsKey(_name)) return;

		for(int i = 0; i < GRID_SIZE; ++i) {
			for(int j = 0; j < GRID_SIZE; ++j) {
				if(cells[i][j].playerName == _name) {
					cells[i][j].playerName = "";
					cells[i][j].color = Color.black;
				}
			}
		}
        //Adds the players name back to the unused name pool
		try {
			useNames.Add (_name);
		} finally {
            deadFile.Write(genomeMap[_name].toPreGenome().ToString());
            deadFile.WriteLine();
            deadFile.Flush();
            sortedList.Remove(genomeMap[_name]);
            genomeMap.Remove (_name);
            turnsUntilMove.Remove(_name);
		}
	}

    //Resets all cells to be enabled
    private void resetBoard() {
        for(int i = 0; i < GRID_SIZE; ++i) {
            for(int j = 0; j < GRID_SIZE; ++j) {
                cells[i][j].active = true;
            }
        }
    }

    private void saveCurrentPlayers() {
        foreach (Genome g in sortedList) {
            endFile.Write(g.toPreGenome().ToString());
            endFile.WriteLine();
        }
        endFile.Flush();
    }

    //TEST FUNCTION DO NOT USE
    private void testAttackDefense() {
        for (int i = 0; i < GameHandler.NUM_ATTACKS; ++i) {
            OutputNeurons current = (OutputNeurons)i;
            print(GameHandler.beats(current) + " beats, " + GameHandler.tiesTo(current) + " ties to, and " + GameHandler.losesTo(current) + " loses to " + current);
        }
    }
}
