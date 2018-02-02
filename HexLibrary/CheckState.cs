﻿using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace HexLibrary
{
    internal struct CheckState
    {
        // 2**CircularMaskLog has to be big enough to hold what you need.  We have sizes at most 26
        // so 2**5 = 32 will do just fine.
        private const int CircularMaskLog = 5;

        private static readonly int CMasks = EdgeTemplate.EdgeTemplates.Length;

        // ReSharper disable InconsistentNaming
        // Indices named by template types as on p. 72
        // ReSharper disable once UnusedMember.Local
        internal const int I = 0;
        internal const int II = 1;
        internal const int IIIa = 2;
        internal const int IIIb = 3;
        internal const int IIIc = 4;
        internal const int IVa = 5;
        internal const int IVb = 6;
        internal const int IVc = 7;
        internal const int Va = 8;
        internal const int Vb = 9;

	    internal const int IM = 1 << I;
		internal const int IIM = 1 << II;
	    internal const int IIIaM = 1 << IIIa;
	    internal const int IIIbM = 1 << IIIb;
	    internal const int IIIcM = 1 << IIIc;
	    internal const int IVaM = 1 << IVa;
	    internal const int IVbM = 1 << IVb;
	    internal const int IVcM = 1 << IVc;
	    internal const int VaM = 1 << Va;
	    internal const int VbM = 1 << Vb;
        // ReSharper restore InconsistentNaming

		private static readonly string[] TemplateNames = new string[]
        {
            "I", "II", "IIIa", "IIIb", "IIIc", "IVa", "IVb", "IVc", "Va", "Vb"
        };

        private static readonly GridLocation[] SideStarts = new[]
        {
            new GridLocation(0, 0),
            new GridLocation(1, 0),
            new GridLocation(0, 1),
            new GridLocation(0, 0)
        };

        private static readonly GridLocation[] RowIncs = new[]
        {
            new GridLocation(0, 1),
            new GridLocation(-1, 0),
            new GridLocation(0, -1),
            new GridLocation(1, 0)
        };

        private static readonly GridLocation[] ColIncs = new[]
        {
            new GridLocation(1, 0),
            new GridLocation(0, 1),
            new GridLocation(1, 0),
            new GridLocation(0, 1)
        };

		/// <summary>
		/// These four variables are the real "heart" of the state.  They indicate where we're currently checking,
		/// what templates are still possible along the edge and where we're searching relative to the potential
		/// templates still alive.  If these values are correct then the state itself is correct.
		/// </summary>
        // Masks showing which templates are alive starting at the corresponding
        // cell.  Templates alive at MaskQueue would have their left cell at
        // _pCol along the corresponding side.
        private readonly CircularQueue<short> _maskQueue;
        // The column within templates in MaskQueue[0] being currently examined
        // ReSharper disable once InconsistentNaming
        private int _tCol;
        // The physical column corresponding to MaskQueue[0].  
        private int _pCol;
        // Current row being inspected
        private int _row;

        private readonly GridLocation _rowInc;
        private readonly GridLocation _colInc;
        private readonly GridLocation _sideStart;

        private readonly Board _board;

        internal CheckState(Board board, int side)
        {
            _board = board;
            _sideStart = (board.Size - 1) * SideStarts[side];
            _colInc = ColIncs[side];
            _rowInc = RowIncs[side];
            _maskQueue = new CircularQueue<short>(CircularMaskLog);
            _maskQueue.Queue(0x3fe);
            _tCol = _pCol = _row = 0;
        }

        internal bool Done => _pCol >= _board.Size - 1;

        internal GridLocation Position => _sideStart + (_pCol + _tCol) * _colInc + _row* _rowInc;

        internal PlayerColor Stone()
        {
            Console.WriteLine($"Getting stone at {Position}");
            return _board[Position];
        }

	    private void AllowOnly(int relIndex, short mask)
	    {
		    if (_tCol >= -relIndex)
		    {
			    _maskQueue[_tCol + relIndex] &= mask;
		    }
	    }

	    private void Eliminate(int relIndex, short mask)
	    {
		    if (_tCol < -relIndex)
		    {
			    return;
		    }
		    Console.WriteLine($"Eliminating {TemplateNames[IndexFromMask(mask)]} from position {_pCol + _tCol + relIndex}");
		    _maskQueue[_tCol + relIndex] &= (short)~mask;
	    }

	    private void AdvanceColumn()
	    {
			++_tCol;
		    _row = 0;
		    _maskQueue.Queue(0x3fe);
		}
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>   Advance the queue by one column. </summary>
		///
		/// <remarks>   Darrell Plank, 1/31/2018. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		private void AdvanceQueue()
	    {
		    _tCol--;
		    _pCol++;
		    _maskQueue.Dequeue();
		    Console.WriteLine($"Advanced to position {_pCol}");
		}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Advance till we find live templates. </summary>
        ///
        /// <remarks>   Darrell Plank, 1/31/2018. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void CheckMask()
        {
            while (_maskQueue[0] == 0 && _tCol > 0)
            {
                AdvanceQueue();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Advance to a relative column index. </summary>
        ///
        /// <remarks>   Normally, this relative index will be negative or zero.
        ///             Darrell Plank, 1/31/2018. </remarks>
        ///
        /// <param name="relIndex"> Zero-based index of the column to move to. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void AdvanceTo(int relIndex)
        {
            if (_tCol <= -relIndex)
            {
                // We're alreay there
                return;
            }

            while (_tCol > -relIndex)
            {
                AdvanceQueue();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
	    /// <summary>   Eliminate templates missing a connecting stone. </summary>
	    ///
	    /// <remarks>   Eliminate any templates that require a connecting stone where we've found an
	    ///             unoccupied cell. Darrell Plank, 1/30/2018. </remarks>
	    ///
	    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
	    ///                                                 invalid. </exception>
	    ////////////////////////////////////////////////////////////////////////////////////////////////////
	    private void EliminateConnectingStone(int row = -1)
	    {
		    if (row < 0)
		    {
			    row = _row;
		    }

            switch (row)
            {
                case 0:
                    break;

				case 1:
                    Eliminate(0, IIM);
                    break;

                case 2:
                    Eliminate(0, IIIaM);
	                Eliminate(-1, IIIbM);
	                Eliminate(-1, IIIcM);
                    break;

                case 3:
	                Eliminate(-1, IVaM);
	                Eliminate(-2, IVbM);
	                Eliminate(-2, IVcM);
                    break;

                case 4:
	                Eliminate(-2, VaM);
	                Eliminate(-3, VbM);
                    break;

                default:
                    throw new InvalidOperationException($"Row set to invalid value in {nameof(EliminateConnectingStone)}");
            }

            // See if we can advance the mask any by virtue of eliminations
            CheckMask();
        }

        // Indexed by length starting at 1 - i.e., use LengthMasks[length - 1]
        private static readonly short[] LengthMasks =
        {
            0b0000000001, //1
            0b0000000010, //2
            0b0000000000, //3
            0b0000001100, //4
            0b0000010000, //5
            0b0000000000, //6
            0b0001100000, //7
            0b0010000000, //8
            0b0000000000, //9
            0b1100000000, //10
        };

        // TmpsByLength[i] is the set of template indices for templates with lengths <= i.
        private static readonly short[] TmpsByLength;

        static CheckState()
        {
            short curMask = 0;
            TmpsByLength = new short[LengthMasks.Length];

            for (var i = 1; i < LengthMasks.Length; i++)
            {
                curMask |= LengthMasks[i];
                TmpsByLength[i] = curMask;
            }
        }

        private static int IndexFromMask(int mask)
        {
            switch (mask)
            {
                case 0b1: return 0;
                case 0b10: return 1;
                case 0b100: return 2;
                case 0b1000: return 3;
                case 0b10000: return 4;
                case 0b100000: return 5;
                case 0b1000000: return 6;
                case 0b10000000: return 7;
                case 0b100000000: return 8;
                case 0b1000000000: return 9;
                default: throw new ArgumentException("mask out of range in IndexFromMask");
            }
        }

        private static IEnumerable<int> IndicesFromMask(int templates)
        {
            var index = 0;
            var mask = 1;
            for (var i = 0; i < CMasks; i++)
            {
                if ((templates & mask) != 0)
                {
                    yield return index;
                }

                index++;
                mask <<= 1;
            }
        }

        private static readonly Dictionary<int, List<int>> TmplSetsToHeights = new Dictionary<int, List<int>>();

        private static int ColumnHeight(int tcol, CircularQueue<short> masks)
        {
            var ret = 0;
            for (var i = 0; i <= tcol; i++)
            {
                if (masks[i] == 0)
                {
                    continue;
                }
                if (!TmplSetsToHeights.ContainsKey(masks[i]))
                {
                    // have to build up the array for this set
                    var htList = new List<int>();

                    foreach (var itmp in IndicesFromMask(masks[i]))
                    {
                        var template = EdgeTemplate.EdgeTemplates[itmp];
                        for (var iCol = 0; iCol < template.ColumnHeights.Length; iCol++)
                        {
                            if (iCol >= htList.Count)
                            {
                                htList.Add(0);
                            }

                            if (template.ColumnHeights[iCol] > htList[iCol])
                            {
                                htList[iCol] = template.ColumnHeights[iCol];
                            }
                        }
                    }
                    TmplSetsToHeights[masks[i]] = htList;
                }

	            var list = TmplSetsToHeights[masks[i]];
	            var index = tcol - i;
	            if (list.Count > index && list[index] > ret)
	            {
		            ret = list[index];
	            }
            }

            return ret;
        }

        internal void ProcessUnoccupied(List<EdgeTemplateConnection> edgeTemplateConnections)
        {
	        if (_row == 0)
	        {
				// No IIIc two columns back
				Eliminate(-2, IIIcM);

				if (_tCol != 0)
		        {
			        var mask = (short) (LengthMasks[_tCol] & _maskQueue[0]);

			        // Check to see if any active templates from state._tCol 0 end here
			        if (mask != 0)
			        {
				        if ((mask & (mask - 1)) != 0)
				        {
					        // There should only be one template instantiated at this point - i.e., the
					        // mask should have exactly one bit.  This is based on the fact that for any given
					        // length there is precisely one template which will fit that length with the
					        // proper connection stone and potentially enemy stones.  This, in turn, is based
					        // on the assumption that any "don't care" positions are actually only recognized
					        // with unfriendly stones there.
					        throw new InvalidOperationException("Internal error in Check");
				        }

				        var templateIndex = IndexFromMask(mask);
				        var tmpl = EdgeTemplate.EdgeTemplates[templateIndex];
				        var connection = (_pCol + tmpl.ConnectStoneColumn) * _colInc +
				                         (tmpl.ColumnHeights[tmpl.ConnectStoneColumn] - 1) * _rowInc;
				        Console.WriteLine($"{TemplateNames[templateIndex]} found at {connection}");
				        edgeTemplateConnections.Add(new EdgeTemplateConnection(templateIndex, connection));

				        // Eliminate this template from contention
				        _maskQueue[0] &= (short) ~mask;

				        // Check to see if there are any more active templates at the original position and 
				        // if not, advance forward until we find more active templates.
				        CheckMask();
			        }
		        }
	        }

	        if (_row == 1)
	        {
				// No IVc three columns back
				Eliminate(-3, IVcM);
	        }

	        // Toss any templates eliminated by the absence of a connecting stone here
            EliminateConnectingStone();

            // Let's move to the next state.Row
            if (++_row >= ColumnHeight(_tCol, _maskQueue))
            {
                // Time to move to the next column
                _tCol++;
                _row = 0;
                // Any templates which fit can potentially be realized from this point
                _maskQueue.Queue(TmpsByLength[Math.Min(TmpsByLength.Length - 1, _board.Size - _pCol - _tCol)]);
            }
        }

        internal void ProcessUnfriendly(List<EdgeTemplateConnection> ret)
        {
			// If it's unfriendly, it's not our connecting stone nor is anything above it
	        for (var iRow = _row; iRow < ColumnHeight(_tCol, _maskQueue); iRow++)
	        {
				EliminateConnectingStone(iRow);
	        }

			// Check out don't care cases
			switch (_row)
			{
				case 0 when _tCol >= 2 && (_maskQueue[_tCol - 2] & IIIcM) != 0:
					// IIIc is the only template possible.  Advance our position to
					// account for this specific template
					AdvanceTo(-2);
					_maskQueue[0] = IIIcM;
					_maskQueue[1] = _maskQueue[2] = 0;
					++_row;
					return;

				case 1 when _tCol >= 3 && (_maskQueue[_tCol - 3] & IVcM) != 0:
					// We're on don't care of IVc. Advance our position to
					// account for this specific template
					AdvanceTo(-3);
					_maskQueue[0] = IVcM;
					_maskQueue[1] = _maskQueue[2] = _maskQueue[3] = 0;
					++_row;
					return;
			}

			// Anywhere but the above don't care cases invalidates all templates it might
			// participate in.

			switch (_row)
	        {
				case 0:
				case 1:
					// All previous templates are invalidated
					_maskQueue.Clear();
					_maskQueue.Queue(0x3fe);
					_pCol += _tCol + 1;
					_tCol = _row = 0;
					break;

				case 2:
					// 2 is not quite as easy a case as 1 because it may be just above the 2 row penultimate column
					// of every tamplate from IIIa on.  We have to allow for all these possibilities at very
					// specific positions.
					AllowOnly(-2, IIIaM | IIIbM);
					AllowOnly(-3, IIIcM);
					AllowOnly(-5, IVaM | IVbM);
					AllowOnly(-6, IVcM);
					AllowOnly(-9, VaM | VbM);
					// We have quite possibly eliminated all possibilities in some columns so check on that
					CheckMask();
					// We've definitely finished off this column
					AdvanceColumn();
					break;

				case 3:
					// Similar to case 2 with different templates at different positions
					AllowOnly(-3, IVaM | IVbM);
					AllowOnly(-4, IVcM);
					AllowOnly(-6, VaM | VbM);
					CheckMask();

					// We've definitely finished off this column
					AdvanceColumn();
					break;

                case 4:
                    // We have to eliminate Va in cols 1, 2 and 3 previous and Vb in cols 2, 3 and 4 previous
                    var index = _tCol - 4;
                    if (index >= 0)
                    {
                        _maskQueue[index] &= ~VbM;
                    }

                    if (++index >= 0)
                    {
                        _maskQueue[index] &= ~VaM & ~VbM;
                    }

                    if (++index >= 0)
                    {
                        _maskQueue[index] &= ~VaM & ~VbM;
                    }

                    if (++index >= 0)
                    {
	                    _maskQueue[index] &= ~VaM;
                    }
                    CheckMask();
	                AdvanceColumn();
                    break;
	        }
		}

		internal void ProcessFriendly(List<EdgeTemplateConnection> ret)
        {
            // Found a connecting stone.  Result depends on row
            switch (_row)
            {
                case 0:
                    // Found a singleton on the edge.  Report it and restart at
                    // the next column
                    ret.Add(new EdgeTemplateConnection(I, Position));

					// Simulate us being at the position of the stone with no previous templates active.
					// MaskQueue has to have exactly one 0 to indicate no templates starting at the
					// current position.  _pCol must be advanced to the point where we found the stone
					// by adding _tCol to it and _tCol must be zero.  The AdvanceColumn at the bottom
					// will then move us past the stone.
					
                    _maskQueue.Clear();
					_maskQueue.Queue(0);
	                _pCol += _tCol;
	                _tCol = 0;
                    break;

                case 1:
                    AdvanceTo(0);
                    _maskQueue[0] = IIM;
                    break;

                case 2:
					AllowOnly(-1, IIIbM | IIIcM);
					AllowOnly(0, IIIaM);
					CheckMask();
                    break;

                case 3:
	                AllowOnly(-1, IVaM);
					AllowOnly(-2, IVbM | IVcM);
					CheckMask();
	                break;

				case 4:
					AllowOnly(-2, VaM);
					AllowOnly(-3, VbM);
					CheckMask();
					break;

				default:
					throw new InvalidOperationException("Advanced too far in ProcessFriendly");
            }

	        AdvanceColumn();
        }

	}
}
