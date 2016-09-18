using System;

public class NeuralSettings {
    public readonly int INPUT_NUM, OUTPUT_NUM, MAX_NEURONS;

    public NeuralSettings(int _input, int _output, int _neurons) {
        INPUT_NUM = _input;
        OUTPUT_NUM = _output;
        MAX_NEURONS = _neurons;
    }
}
