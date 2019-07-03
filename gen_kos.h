#pragma once

#ifndef _GEN_KOS_H
#define _GEN_KOS_H

namespace Kos {
namespace Gen {
;
extern const double c_0707;
extern const double sqrt_2;
extern const double Pi;

double  erf_( double x );
double  Int_Gaus( double x, double a, double b, double G );
void  Render_Point( double frame[], int pitch, int fx, int fy, int lx, int ly, double xp, double yp, double Et, double Spsf );
void  Render_Line( double frame[], int pitch, int fx, int fy, int lx, int ly, double xa, double ya, double xb, double yb, double Et, double Spsf, double spp );
void  Get_Norm_Noise( double &z0, double &z1 );

}}

#endif//_GEN_KOS_H
