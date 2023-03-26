using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace RollArray {
    public class RollArray<T>
    {
        private T[] _data; // array data
        public int _length = 0; // number of items in array
        public int _head = 0; // current pointer in array
        public int _capacity; // maximum size of array

        public RollArray(int capacity) {
            _data = new T[capacity];
            _capacity = capacity;
        }

        public void push(T item) {
            lock(this) {
                _data[_head] = item; // append to array
                _head = (_head + 1) % _capacity; // check for roll-over index
                if (_length < _capacity) { // clip length
                    _length++;
                }
            }
        }

        // https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
        private int mod(int x, int m) { // for roll over indices given negative 'x'
            return (x%m + m)%m;
        }

        public T[] slice(int start, int stop) {
            lock(this) {
                int _start = mod(start, _length); // compute start index
                int _stop = mod(stop, _length); // compute stop index
                int _idx = 0; // index to start copying to in destination array
                int _size = _stop - _start; // length of destination array

                if (_size < 0) { // check if we having roll-over indices
                    _size = (_length - _start) + _stop;
                }

                // Debug.Log("size: " + _size + " start: " + _start + " stop: " + _stop);

                T [] _slice = new T[_size]; // allocate destination array

                if (_start > _stop) {  // for roll-over copy first-half to destination array
                    int diff  = _length - _start;
                    Array.Copy(_data, _start, _slice, _idx, diff);
                    _idx = diff;
                    _start = 0;
                    _size -= _idx;
                }

                // Debug.Log(_idx + " " + _size);

                Array.Copy(_data, _start, _slice, _idx, _size); // copy to destination array
                return _slice;
            }
        }
    }
}