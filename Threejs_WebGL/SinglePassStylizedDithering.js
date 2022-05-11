/* Experimental Circle Cell Shaders
 * Jennifer Fullerton
 * jfTestShaders.js
 * Creates a single-pass dithering and cell shading effect.
 * Made with Three.js & WebGL
 *
 * Uses simple equations to manipulate diffuse and specular values to create a real-time
 * dithering effect.  Adjusts with camera position.
 */


var jfCircVert = `
	precision mediump float;
	// *** RAW SHADER GEOMETRY AND CAMERA UNIFORMS *** //
	// if using RawShaderMaterial, we need these uniforms

	// camera uniforms
	uniform mat4 modelMatrix;
	uniform mat4 viewMatrix;
	uniform mat4 projectionMatrix;

	// geometry attributes and uniforms
	attribute vec3 position;
	attribute vec3 normal;

	// *** LIGHTING *** //
	// light uniforms
	const vec3 light1_pos = vec3(0.0, 10.0, 1.0);
	const vec3 light2_pos = vec3(5.0, 5.0, 5.0);

	// fragment shader variables
	varying vec3 v_pos;
	varying vec3 N, L1, L2, V;

	void main()
	{
		// vertex position in camera coordinates
		vec4 pos = viewMatrix * modelMatrix * vec4(position, 1.0);
		// normalized vertex normal - camera coordinates
		N = vec3( normalize( viewMatrix * modelMatrix * vec4(normal.xyz, 0.0) ).xyz );
		// send position info to frag shader
		v_pos = pos.xyz;

		// * convert light coordinates from world to camera
		// 		LIGHT 1
		vec4 L1_cam = viewMatrix * vec4(light1_pos, 1.0);
		// calculate normalized light direction vector
		L1 = vec3(normalize( L1_cam - pos ).xyz);
		//		LIGHT 2
		vec4 L2_cam = viewMatrix * vec4(light2_pos, 1.0);
		L2 = vec3(normalize( L2_cam - pos ).xyz);
		// calculate view vector from position
		V = normalize(-v_pos);

		// final adjustment using projection matrix
		gl_Position = projectionMatrix * pos;
	}`;

var jfCircFrag = `
	precision mediump float;
	// * VARYINGS * //
	varying vec3 v_pos;
	varying vec3 N, L1, L2, V;

	// * LIGHTING CONSTANTS * //
	const vec3 ambient = vec3(0.1);
		// DIFFUSE - Phong Model
	const vec3 kd1 = vec3(1.0, 0.0, 0.0);	// light 1 diffuse color
	const vec3 kd2 = vec3(0.0, 0.0, 0.9);	// light 2 diffuse color
		// SPECULAR - Phong Model
	const vec3 ks1 = vec3(1.0, 1.0, 0.0);	// light 1 spec color
	const vec3 ks2 = vec3(0.0, 1.0, 1.0); 	// light 2 spec color
	const float spec_intensity = 2.5;		// specular gloss
		// STYLE MODIFIERS
	const float radius = 1.0;	// specular highlight radius
		// controls for drawing specular halftone dots
	float cx, sy, ksModifier, rad1, rad2;

	// *** FUNCTIONS *** //

	// *	compDiffuse() * //
	//	calculates the stylized diffuse based on the Phong lighting model.
	//	return: vec3(diffuse.rgb)
	vec3 compDiffuse(vec3 L, vec3 KD){
		vec3 diffuseRGB = vec3(0.0);
		// cx = cos of x coord, sy = sin of y coord for gl_FragCoord
		// used to calculate / create the dithering circles effect from specular
		float cx, sy;

		// calculate angle between light and vertex normal
		float diff1 = max(0.0, dot(N, L));

		// create toon shading using thresholds
		for(float i = 0.0; i<=1.0; i+= 0.1){
			if(diff1 >= i){
				cx = cos(gl_FragCoord.x);
				sy = sin(gl_FragCoord.y);
				// use cx sy to create dithering / halftone pattern
				if( cx*cx + sy*sy >= diff1 ){
					// use threshold (i) as diffuse value
					diffuseRGB = vec3(KD * i);
				}
			}
		}
		
		return diffuseRGB;
	}
	// * compSpecular() * //
	//	calculates the stylized specular based on the Phong
	//		lighting model.
	//	return: vec3(specular.rgb)
	vec3 compSpecular(vec3 L, vec3 KS)
	{
		vec3 specRGB = vec3(0.0);
		float cx, sy;
		// N and V constant
		vec3 R = normalize( reflect(-L,N) );
		
		// find dot product between reflection and view
		float S;
		float RdotN = dot(R, V);
		// if positive, then calculate normally
		if(RdotN >= 0.0){
			S = pow( max(dot(R,V), 0.0), spec_intensity );
		}
		// if dot product is negative, then weird specular
		else {
			// negate the dot product so it returns as a "max"
			S = pow( max(-dot(R,V), 0.0), spec_intensity );
			S = -S;	// negate specular
		}
		// adjust radius based on specular
		float rad = radius * S;
		// normal specular
		if (S > 0.0){
			cx = cos(gl_FragCoord.x)*rad;
			sy = sin(gl_FragCoord.y)*rad;
			// smaller threshold = less visible
			if( cx*cx +  sy*sy >= rad )
				specRGB += vec3( KS * S);
		} 
		// "negative" specular
		if (S < 0.0){
			rad = -rad;
			cx = cos(gl_FragCoord.x)*rad;
			sy = sin(gl_FragCoord.y)*rad;
			if( cx*cx +  sy*sy >= rad )
				specRGB += vec3( KS * S);
		}
		return specRGB;
	}
	void main()
	{
		// default light = 0.0
		vec4 outColor1 = vec4(ambient, 1.0);
		// DIFFUSE
		vec3 diff1 = compDiffuse(L1, kd1);
		vec3 diff2 = compDiffuse(L2, kd2);
		outColor1 += vec4(diff1 + diff2, 1.0);
		
		// SPECULAR CIRCLES
		vec3 specRes1 = compSpecular(L1, ks1);
		vec3 specRes2 = compSpecular(L2, ks2);
		outColor1 += vec4(specRes1 + specRes2, 1.0);
		gl_FragColor = outColor1;
	}`;