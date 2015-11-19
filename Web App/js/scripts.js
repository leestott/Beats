    var data;
    
    setInterval(updatevars, 1000);
        
        function updatevars(){
    $.ajax(
    {
        url: "http://beatsapi.azurewebsites.net/api/SentimentData/beats",
        type: 'GET',
        dataType: 'json',
        accept: 'application/json',
        success: function(data)
        {  
            console.log(data);
            var objets= data; // $.parseJSON(data);
    
            $.each(objets, function(i, obj)
            {
               console.log(obj.title);
            });
    
        
            document.getElementById("heartbeat_value").innerHTML = data.Heart.toString();    
            document.getElementById("steps_value").innerHTML = data.Steps.toString();    
            document.getElementById("temperature_value").innerHTML = data.Temp.toString();     
                        
        }
    });   
        };
               
    
    