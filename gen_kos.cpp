//#define _USE_MATH_DEFINES
#include <math.h>
#include <stdlib.h>

#include "gen_kos.h"

#ifndef _sqr
#define _sqr(a) ((a)*(a))
#endif

namespace Kos {
namespace Gen {
;

/*extern*/ const double c_0707	= 0.70710678118654752440084436210485;
/*extern*/ const double sqrt_2	= 1.4142135623730950488016887242097;
/*extern*/ const double Pi		= 3.1415926535897932384626433832795;

//-- from Detection unit ----------------------------------------------------
/*******************************    erf_    ********************************}
{*         for given "x" calculates standard function "erf(x)"             *}
{***************************************************************************}
{* CALLED FROM  : int_gauss                                                *}
{* CALLS        : NONE                                                     *}
{* INPUT        : x                                                        *}
{* OUTPUT       : erf_                                                     *}
{* MODIFIES     : NONE                                                     *}
{* RETURNS      : value of erf(x)                                          *}
{* GLOBAL       : NONE                                                     *}
{***************************************************************************/
double  erf_( double x ) {
	double	t,y;
	bool	s = (x>=0.0);
			x = fabs(x);
	if( x < 6.0 ) {
		t	= 1.0 / ( 1.0 + 0.47047*x );
		y	= 0.3480242*t - 0.0958798*t*t + 0.7478556*t*t*t;
		y	= 1 - y*exp(-x*x);
	} else y = 1.0;
	return (s?y:-y);
}

//-- from Detection unit ----------------------------------------------------
/*******************************  int_gauss  *******************************}
{*           calculates integral within the limits (a,b) from              *}
{*   Gaussian probability dencity function with mean <x> and variance <G>  *}
{***************************************************************************}
{* CALLED FROM  : init_prec_tab, prec_est_sub, prec_est_opt                *}
{* CALLS        : erf_                                                     *}
{* INPUT        : x,a,b,G                                                  *}
{* OUTPUT       : int_gauss                                                *}
{* MODIFIES     :                                                          *}
{* RETURNS      :                                                          *}
{* GLOBAL       : NONE                                                     *}
{***************************************************************************/
double  Int_Gaus( double x, double a, double b, double G ) {
	double	c = c_0707/G;
	return	(erf(c*(b-x))-erf(c*(a-x)))*0.5;
}

//------------------------------------------------------------------------------
// Render point light at (xp,yp) on the 'frame'
//  within boundaries (fx,fy)-(lx,ly) inclusive
//  according to the Gaussian PSF with variance 'Spsf'
//  normed by the total radiated energy 'Et'
//------------------------------------------------------------------------------
void  Render_Point( double frame[], int pitch, int fx, int fy, int lx, int ly, double xp, double yp, double Et, double Spsf ) {
	double	s	= sqrt_2 * Spsf;
	double	c	= c_0707 / Spsf;
	double  RL  = 6.0*s + 0.5;

	int		jy_f= (int)floor( -RL +yp ) + 1;
	int		jy_l= (int) ceil(  RL +yp ) - 1;
			jy_f=		_max(  jy_f, fy );
			jy_l=		_min(  jy_l, ly );

	int		ix_f= (int)floor( -RL +xp ) + 1;
	int		ix_l= (int) ceil(  RL +xp ) - 1;
			ix_f=		_max(  ix_f, fx );
			ix_l=		_min(  ix_l, lx );

	if(	( jy_l < jy_f ) || ( ix_l < ix_f ) )
		return;

	double *erf_liney = new double[jy_l-jy_f+2];

	double *erf_linex = new double[ix_l-ix_f+2];

	for( int jy = jy_f-1; jy <= jy_l; ++jy ) {
		double t = ( (double)jy + 0.5 - yp ) * c;
		erf_liney[jy-jy_f+1] = erf_(t);
	}

	for( int ix = ix_f-1; ix <= ix_l; ++ix ) {
		double t = ( (double)ix + 0.5 - xp ) * c;
		erf_linex[ix-ix_f+1] = erf_(t);
	}

	Et *= 0.25;

	for( int jy = jy_f; jy <= jy_l; ++jy )
		for( int ix = ix_f; ix <= ix_l; ++ix ) {
			frame[jy*pitch+ix] += Et * (erf_liney[jy-jy_f+1] - erf_liney[jy-jy_f]) 
									 * (erf_linex[ix-ix_f+1] - erf_linex[ix-ix_f]);
		}

	delete[]erf_linex;
	delete[]erf_liney;
}

//------------------------------------------------------------------------------
// Render trace of the uniformly straight line moving point light on the 'frame'
//  from the point (xa,ya) to the point (xb,yb)
//  within boundaries (fx,fy)-(lx,ly) inclusive
//  according to the Gaussian PSF with variance 'Spsf'
//  splitting the length of the trajectory by the 'spp' steps per pixel
//  with amplitude 'Et' at the central points
//------------------------------------------------------------------------------
void  Render_Line( double frame[], int pitch, int fx, int fy, int lx, int ly, double xa, double ya, double xb, double yb, double Et, double Spsf, double spp ) {
	double	xl		= xb-xa;
	double	yl		= yb-ya;
	double	len		= sqrt( _sqr(xl) + _sqr(yl) );
	int		kmax	= (int)floor( len * spp ) +1;
			Et		/= spp;

	for( int k = 0; k <= kmax; ++k ) {
		double tk = (double)k/(double)kmax;
		double xk = xl * tk + xa;
		double yk = yl * tk + ya;
		Render_Point( frame, pitch, fx,fy,lx,ly, xk,yk, Et,Spsf );
	}
}

void  Get_Norm_Noise( double &z0, double &z1 ) {
	static const double RM2I = 2.0/(double)RAND_MAX;
	double x,y,s,r;

	do {
		x = RM2I*(double)rand() - 1.0;
		y = RM2I*(double)rand() - 1.0;
		s = _sqr(x)+_sqr(y);
	} while
		( (s==0.0) || (s>1.0) );

	r = sqrt( -2.0 * log(s) / s );

	z0 = x*r;
	z1 = y*r;
}

}}
