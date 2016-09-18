using System.Collections.Generic;

//Represents a neuron which has an outputting value and a list of incoming neural connections
public class Neuron {
    //List of neurons for which this neuron is connected via an axon. This is used to determine the incoming value
    private List<Gene>   _incoming;
    //The current value of this neuron.
    private double       _value;
    
    public Neuron() {
        _incoming = new List<Gene>();
        _value = 0;
    }
    
    public List<Gene> incoming {
        get {
            return _incoming;
        }
        set {
            _incoming = value;
        }
    }

    public double value {
        get {
            return _value;
        }
        set {
            _value = value;
        }
    }
    
}
