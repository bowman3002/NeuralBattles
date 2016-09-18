using System;

public class Gene {
    //This is whether the gene is enabled or not
    public bool _enabled;
    //This gene is dominant
    public bool _dominant;
    //This gene holds the dendrite for this number neuron
    public int _in;
    //This gene holds the axon for this number neuron
    public int _out;
    //This is the number in which the gene was in order of development
    private int _innovation;
    
    //This is the weight of the connection between two neurons
    private double _weight;
    
    //Copy constructor for Gene. 
	public Gene(Gene g) {
		_enabled = g._enabled;
		_dominant = g._dominant;
		_in = g._in;
		_out = g._out;
		_innovation = g._innovation;
		_weight = g._weight;
	}

    //Member specific constructor for Gene
    public Gene(int i_in, int i_out, int i_innovation, double i_weight) {
        _enabled = true;
        _in = i_in;
        _out = i_out;
        _innovation = i_innovation;
        _weight = i_weight;
    }
    
    //String decoding constructor for Gene
    public Gene(string g) {
        string[] stringList = g.Split(',');
        _enabled = Convert.ToBoolean (stringList[0]);
        
        _in = Convert.ToInt32(stringList[1]);
        _out = Convert.ToInt32 (stringList[2]);
        _innovation = Convert.ToInt32(stringList[3]);
        _weight = Convert.ToDouble(stringList[4]);
        
        _dominant = Convert.ToBoolean(stringList[5]);
    }
    
    public bool enabled {
        get {
            return _enabled;
        }
        set {
            _enabled = value;
        }
    }

    public int inNode {
        get {
            return _in;
        }
        set {
            _in = value;
        }
    }

    public int outNode {
        get {
            return _out;
        }
        set {
            _out = value;
        }
    }

    public int innovation {
        get {
            return _innovation;
        }
        set {
            _innovation = value;
        }
    }

    public double weight {
        get {
            return _weight;
        }
        set {
            _weight = value;
        }
    }

    public bool dominant {
        get {
            return _dominant;
        }
        set {
            _dominant = value;
        }
    }
    
    
    //Encodes gene into a string that can be read in and decoded by the string constructor
    override public string ToString() {
        return enabled + "," + _in + "," + _out + "," + innovation + "," + weight + "," + dominant + ";";
    }
}
