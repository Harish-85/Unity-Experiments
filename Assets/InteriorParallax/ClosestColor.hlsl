

void GetClosestColor_float(float dist1,float4 col1,float dist2,float4 col2, float dist3, float4 col3,float dist4,float4 col4,float dist5,float4 col5,out float4 color)
{
    dist1 = dist1<.001f ? 1000000 : dist1;
    dist2 = dist2<.001f ? 1000000 : dist2;
    dist3 = dist3<.001f ? 1000000 : dist3;
    dist4 = dist4<.001f ? 1000000 : dist4;
    dist5 = dist5<.001f ? 1000000 : dist5;
    
    if(  dist1 < dist2 && dist1 < dist3 && dist1 < dist4 && dist1 < dist5)
    {
        color = col1;
    }
    else if(dist2 < dist1 && dist2 < dist3 && dist2 < dist4 && dist2 < dist5)
    {
        color = col2;
    }
    else if(dist3 < dist1 && dist3 < dist2 && dist3 < dist4 && dist3 < dist5)
    {
        color = col3;
    }
    else if(dist4 < dist1 && dist4 < dist2 && dist4 < dist3 && dist4 < dist5)
    {
        color = col4;
    }
    else if(dist5 < dist1 && dist5 < dist2 && dist5 < dist3 && dist5 < dist4)
    {
        color = col5;
    }
    
    /*else if(dist2 < dist1 && dist2 < dist3 && dist2 < dist4)
    {
        color = col2;
    }
    else if(dist3 < dist1 && dist3 < dist2 && dist3 < dist4)
    {
        color = col3;
    }
    else if(dist4 < dist1 && dist4 < dist2 && dist4 < dist3)
    {
        color = col4;
    }*/
    
}
 