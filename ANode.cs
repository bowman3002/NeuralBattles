using System.Collections;
using System.Collections.Generic;

//A node holding value T and connections to any number of children index by a double
public class ANode<T> {
	private T 								_value;
	private Dictionary<double, ANode<T>> 	childNodes;

	public ANode() {
		_value = default(T);
		childNodes = new Dictionary<double, ANode<T>>();
	}

	public ANode<T> getChild(double key) {
		ANode<T> nextNode = null;
		childNodes.TryGetValue (key, out nextNode);
		return nextNode;
	}

    public T value {
        get {
            return _value;
        }
        set {
            _value = value;
        }
    }

	public ANode<T> addChild(double key) {
		childNodes.Add (key, new ANode<T>());
		return childNodes[key];
	}
}
