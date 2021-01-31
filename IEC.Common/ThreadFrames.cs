using System;
using System.Collections.Generic;
using System.Linq;

namespace IEC.Common
{
    public class ThreadFrames
    {
        private readonly List<IThreadFrame> _frames = new List<IThreadFrame>();
        
        private int _currentIndex = 0;

        public ThreadFrames(
            )
        {
        }

        public void AddFrame(
            IThreadFrame frame
            )
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (_frames.Count != _currentIndex)
            {
                _frames.RemoveRange(_currentIndex, _frames.Count - _currentIndex);
            }

            _frames.Add(frame);
            _currentIndex++;
        }

        public void PopFrame(IThreadFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (_frames.Count == 0)
            {
                throw new InvalidOperationException("No frames to pop!");
            }
            if (!ReferenceEquals(_frames[_currentIndex - 1], frame))
            {
                throw new InvalidOperationException("Incoming frame is not last!");
            }

            _currentIndex--;
        }

        public List<IThreadFrame> ExtractFrames()
        {
            return new List<IThreadFrame>(_frames);
        }
    }
}
