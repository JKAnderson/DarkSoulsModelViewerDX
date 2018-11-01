﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DebugPrimitives
{
    public class DbgPrimWireSphere : DbgPrimWire
    {
        public DbgPrimWireSphere(Transform location, float radius, int numVerticalSegments, int numSidesPerSegment, Color color)
        {
            if (!(numVerticalSegments >= 2))
                throw new ArgumentException($"Number of vertical segments must be >= 2", nameof(numVerticalSegments));
            if (!(numSidesPerSegment >= 3))
                throw new ArgumentException($"Number of sides per vertical segment must be >= 3", nameof(numSidesPerSegment));

            numVerticalSegments -= 1;

            var topPoint = Vector3.Up * radius;
            var bottomPoint = Vector3.Down * radius;
            var points = new Vector3[numVerticalSegments, numSidesPerSegment];

            for (int i = 0; i < numVerticalSegments; i++)
            {
                for (int j = 0; j < numSidesPerSegment; j++)
                {
                    float horizontalAngle = (1.0f * j / numSidesPerSegment) * MathHelper.TwoPi;
                    float verticalAngle = ((1.0f * (i + 1) / (numVerticalSegments + 1)) * MathHelper.Pi) - MathHelper.PiOver2;
                    float altitude = (float)Math.Sin(verticalAngle);
                    float horizontalDist = (float)Math.Cos(verticalAngle);
                    points[i, j] = new Vector3((float)Math.Cos(horizontalAngle) * horizontalDist, altitude, (float)Math.Sin(horizontalAngle) * horizontalDist) * radius;
                }
            }

            for (int i = 0; i < numVerticalSegments; i++)
            {
                for (int j = 0; j < numSidesPerSegment; j++)
                {
                    // On the bottom, we must connect each to the bottom point
                    if (i == 0)
                    {
                        AddLine(points[i, j], bottomPoint, color);
                    }
                    // On the top, we must connect each point to the top
                    // Note: this isn't "else if" because with 2 segments, 
                    // these are both true for the only ring
                    if (i == numVerticalSegments - 1)
                    {
                        AddLine(points[i, j], topPoint, color);
                    }

                    // Make vertical lines that connect from this 
                    // horizontal ring to the one above
                    // Since we are connecting 
                    // (current) -> (the one above current)
                    // we dont need to do this for the very last one.
                    if (i < numVerticalSegments - 1)
                    {
                        AddLine(points[i, j], points[i + 1, j], color);
                    }


                    // Make lines that connect points horizontally
                    //---- if we reach end, we must wrap around, 
                    //---- otherwise, simply make line to next one
                    if (j == numSidesPerSegment - 1)
                    {
                        AddLine(points[i, j], points[i, 0], color);
                    }
                    else
                    {
                        AddLine(points[i, j], points[i, j + 1], color);
                    }
                }
            }

        }
    }
}
