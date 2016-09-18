using System.Collections.Generic;
using System;

//Simulates a NeuralNetwork and its decisions based on inputs
public class NeuralNetwork {
    
    private List<Neuron> neurons;

	private ANode<List<double>> previousInputs;

    private NeuralSettings _settings;
    
    //Creates a NeuralNetwork from a genome
    public NeuralNetwork(Genome genome, NeuralSettings i_settings) {
        _settings = i_settings;

		previousInputs = new ANode<List<double>> ();
        
		neurons = new List<Neuron> ();
		neurons.Capacity = _settings.MAX_NEURONS + _settings.OUTPUT_NUM;
		for (int i = 0; i < _settings.MAX_NEURONS + _settings.OUTPUT_NUM; ++i) {
			neurons.Add (null);
		}
        
        //Adds all of the input neurons
        for(int i = 0; i < _settings.INPUT_NUM; ++i) {
            neurons[i] = new Neuron();
        }
        
        //Adds all of the output neurons
        for(int i = _settings.MAX_NEURONS; i < neurons.Count; ++i) {
            neurons[i] = new Neuron();
        }
        
        //Adds all of the connections along with the hidden neurons
        foreach(Gene g in genome.phenotype) {
            if(!g.enabled) continue;
            
            int _out = g.outNode;
            int _in  = g.inNode;
            
            if(neurons[_out] == null) {
                neurons[_out] = new Neuron();
            }
            
            if(neurons[_in] == null) {
                neurons[_in] = new Neuron();
            }
            
            neurons[_out].incoming.Add(g);
        }
    }
    
    //Recursively evaluates the neuron with index i outputting its output value
    public double evaluateNeuron(int i) {
        return evaluateNeuron(neurons[i]);
    }
    
    //Recursively evaluates neuron n outputting its output value
    public double evaluateNeuron(Neuron n) {
		if (n == null) return 0;

        List<Gene> incoming = n.incoming;
        
        if(incoming.Count == 0) {
            if(neurons.IndexOf(n) < _settings.INPUT_NUM) {
                //n is an input neuron which already had a value set
                return n.value;
            } else {
                return 0;
            }
        }
        
        double value = 0;
        foreach(Gene g in n.incoming) {
            value += evaluateNeuron(neurons[g.inNode]) * g.weight;
        }
        
        return value;
    }
    
    //Copies new input values into the input neurons for this network
    public void setInputs(List<double> values) {
        if(values.Count != _settings.INPUT_NUM) {
			return;
        }
        
        for(int i = 0; i < _settings.INPUT_NUM; ++i) {
            neurons[i].value = values[i];
        }
    }
    
    //Evaluates entire network and returns a list of the output values
    public List<double> evaluateNetwork(List<double> inputs) {
        //Attempts to use a tree to retrieve outputs if these exact inputs have already been seen
		List<double> possibleOutputs = evaluatePrevious (inputs, previousInputs);

		if (possibleOutputs != null) {
            //We've already encountered this exact input list so we know the outputs
			return possibleOutputs;
		}

        for(int i = 0; i < _settings.INPUT_NUM; ++i) {
            //Reset all inputs
            neurons[i].value = 0;
        }

		setInputs(inputs);
        
        List<double> values = new List<double>();
        
        //Loop through all output nodes
        for(int i = _settings.MAX_NEURONS; i < _settings.MAX_NEURONS + _settings.OUTPUT_NUM; ++i) {
            values.Add(evaluateNeuron(i));
        }

		addOutputs (inputs, values, previousInputs);
        
        return values;
    }

    //Return a random neuron. 
    //nonInput = false includes input nodes and excludes output nodes.
    //nonInput = true includes output nodes and excludes input nodes.
    public int randomNeuron(bool nonInput) {
        List<int> compactList = new List<int>();
        
        if(!nonInput) {
            //Include input nodes
            for(int i = 0; i < _settings.INPUT_NUM; ++i) {
                compactList.Add(i);
            }
        }
        
		if (nonInput) {
            //Include output nodes
			for (int i = _settings.MAX_NEURONS; i < _settings.MAX_NEURONS + _settings.OUTPUT_NUM; ++i) {
				compactList.Add (i);
			}
		}
        
        for(int i = _settings.INPUT_NUM; neurons[i] != null; ++i) {
            //Add all other nodes
            compactList.Add(i);
        }
        
        Random r = new Random();
        
        return compactList[r.Next(compactList.Count)];
    }
    
    //Returns the lowest index neuron that is null
    public int nextNeuron() {
        int i = _settings.INPUT_NUM;
        while(neurons[i] != null) {
            ++i;
        }
        return i;
    }

    //Attempts to recall if this exact input has been seen before
	public List<double> evaluatePrevious(List<double> input, ANode<List<double>> tree) {
		if (input.Count == 0) {
			return tree.value;
		} else {
			ANode<List<double>> nextTree = tree.getChild (input [0]);
			if(nextTree == null) {
				return null;
			}

			List<double> newInput = new List<double>(input);
			newInput.RemoveAt (0);

			return evaluatePrevious(newInput, nextTree);
		}
	}

    //Adds the output to the dynamic programming tree for this specific input
	public void addOutputs(List<double> input, List<double> output, ANode<List<double>> tree) {
		if (input.Count == 0) {
			tree.value = output;
		} else {
			ANode<List<double>> nextNode = tree.getChild (input [0]);
			double value = input[0];

			if(nextNode == null) {
				tree.addChild(value);
			}

			List<double> newInput = new List<double>(input);
			newInput.RemoveAt(0);

			addOutputs (newInput, output, tree.getChild(value));
		}
	}
}
